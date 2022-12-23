using UnityEngine;

public abstract class FishState
{
    public Fish _context;
    protected Rigidbody _rb;
    public FishState(Fish context)
    {
        _context = context;
        _rb = _context.GetComponent<Rigidbody>();
    }
    public virtual void Update() { }
    public virtual void FixedUpdate()
    {
        _rb.rotation = Quaternion.AngleAxis(_rb.rotation.eulerAngles.y, Vector3.up);
    }
    public virtual void HandleCollideWithFood(FishFood food) { }
    public virtual void HandleCollideWithOtherObjects(Collision collision) { }
    public virtual void HandleCollidedWithWater() { }
    public virtual void OnFoodConsumed() { }
}

public abstract class TransitionalState : FishState
{
    protected FishState _nextState;

    public TransitionalState(Fish context, FishState transitionInto) : base(context)
    {
        _nextState = transitionInto;
    }
}

public class DroppingIntoTankState : FishState
{
    private float _maxDrag = 70f;
    private float _dragDuration;
    private float _rotateDuration;

    public DroppingIntoTankState(Fish context) : base(context)
    {
        _dragDuration = Random.Range(0.25f, 0.75f);
        _rotateDuration = Random.Range(0.5f, 0.75f);
    }

    public override void HandleCollidedWithWater()
    {
        if (_rb.drag >= _maxDrag)
        {
            _rb.drag = 0f;
            _context.TogglePhysics(false);
            var angle = Random.Range(0f, 180f);

            FishState nextState = _context.IsFull ?
                new WanderingAndFullState(_context) :
                    new WanderingAndHungryState(_context);
            _context.State = new RotateTransitionalState(
                _context, Quaternion.AngleAxis(angle, Vector3.up), _rotateDuration, nextState);
            PlaceablesManager.Instance.SavePlaceables();
            return;
        }
        // should stop after 0.4s
        _rb.drag += _maxDrag * (1 / _dragDuration) * Time.fixedDeltaTime;
    }
}

public class RotatingState : FishState
{
    protected Quaternion _endRotation;
    protected Quaternion _startRotation;
    protected float _duration;
    protected float _timeProgress = 0f;
    protected bool _rotating;
    private bool _damping = true;

    public RotatingState(Fish context, Quaternion rotation, float duration = -1f) : base(context)
    {
        _endRotation = rotation;
        _startRotation = _rb.rotation;
        _duration = duration;
        _rotating = true;
    }

    public override void FixedUpdate()
    {
        if (_damping && _rb.angularVelocity.sqrMagnitude > 0.0001f)
        {
            _rb.angularDrag += 50f;
            return;
        }
        else if (_damping)
        {
            _rb.angularDrag = 0f;
            _damping = false;
        }

        if (_duration > 0f)
        {
            var clampedDeltaTime = Mathf.Clamp(Time.fixedDeltaTime, 0f, _duration - _timeProgress);
            _rb.rotation = Quaternion.Slerp(_startRotation, _endRotation, _timeProgress / _duration);
            _timeProgress += clampedDeltaTime;
        }
        else _rb.rotation = Quaternion.RotateTowards(_rb.rotation, _endRotation, Time.fixedDeltaTime);
    }
}

public class RotateTransitionalState : TransitionalState
{
    private float _duration;
    private float _timeProgress = 0f;
    private RotatingState _rotateState;

    public RotateTransitionalState(
        Fish context, Quaternion rotation, float duration, FishState transitionInto) : base(context, transitionInto)
    {
        _duration = duration;
        _rotateState = new RotatingState(_context, rotation, _duration);
    }

    public override void FixedUpdate()
    {
        if (_timeProgress == _duration)
        {
            _context.State = _nextState;
            return;
        }
        base.FixedUpdate();
        var clampedDeltaTime = Mathf.Clamp(Time.fixedDeltaTime, 0f, _duration - _timeProgress);
        _timeProgress += clampedDeltaTime;
    }
}

public abstract class WanderingState : RotatingState
{
    private float _movementPhase = 0f;
    private float _movementProgress = 0f;
    private float _movementModifier;

    public WanderingState(Fish context) : base(context, context.transform.rotation)
    {
        _rotating = false;
    }

    public override void FixedUpdate()
    {
        if (_rotating && _timeProgress < _duration)
            base.FixedUpdate();
        else if (_timeProgress == _duration)
            _rotating = false;
        else
        {
            var rotate = Random.Range(0f, 10f);
            if (rotate > 7f)
            {
                var angle = Random.Range(30f, 120f);
                _startRotation = _context.transform.rotation;
                _endRotation = _startRotation * Quaternion.AngleAxis(angle, Vector3.up);
                _duration = Random.Range(1f, 2.5f);
                _timeProgress = 0f;
                _rotating = true;
            }
        }

        if (_movementProgress == _movementPhase)
        {
            if (_rb.velocity.sqrMagnitude < 1f)
            {
                _movementPhase = Random.Range(1f, 3f);
                _movementModifier = Random.Range(0.8f, 1.2f);
            }
            else
            {
                _movementPhase = Random.Range(5f, 7.5f);
                _movementModifier = 0f;
            }
            _movementProgress = 0f;
        }
        if (_movementModifier == 0f && _rb.velocity.sqrMagnitude > 0.04f)
        {
            _rb.drag += 20f;
        }
        else _rb.velocity = -_context.transform.right * _movementModifier * _context.SwimSpeed;
        _movementProgress += Time.fixedDeltaTime;
    }

    public override void HandleCollideWithOtherObjects(Collision collision)
    {
        if (collision.collider.CompareTag(FishTank.GlassTag))
        {
            var angle = Random.Range(90f, 180f);
            _context.State = new RotateTransitionalState(
                _context, Quaternion.AngleAxis(angle, Vector3.up), 0.5f, this);
        }
    }
}

public class WanderingAndHungryState : WanderingState
{
    public WanderingAndHungryState(Fish context) : base(context) { }
    public override void Update()
    {
        var food = _context.DetectFood();
        if (food)
            _context.State = new HeadingForFoodState(_context, food);
        else if (!_context.CheckIsAlive())
            _context.State = new DeadState(_context);
    }
}

public class WanderingAndFullState : WanderingState
{
    public WanderingAndFullState(Fish context) : base(context) { }
    public override void Update()
    {
        if (!_context.IsFull)
            _context.State = new WanderingAndHungryState(_context);
    }
}

public class HeadingForFoodState : RotatingState
{
    private FishFood _food;

    public HeadingForFoodState(Fish context, FishFood food) : base(context, food.transform.rotation, 0.5f)
    {
        _food = food;
    }

    public override void FixedUpdate()
    {
        var angleToForwardDirection = 90f;
        var rotationToFood =
                Quaternion.LookRotation(_food.transform.position - _context.transform.position)
                    * Quaternion.AngleAxis(angleToForwardDirection, Vector3.up);
        _endRotation = rotationToFood;
        base.FixedUpdate();
        _context.SwimToFood();
    }

    public override void HandleCollideWithFood(FishFood food)
    {
        _context.ConsumeFood(food);
    }

    public override void OnFoodConsumed()
    {
        _context.State = _context.IsFull ?
            new WanderingAndFullState(_context) : new WanderingAndHungryState(_context);
    }
}

public class DeadState : FishState
{
    public DeadState(Fish context) : base(context)
    {
        var rb = _context.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationZ;
        rb.MoveRotation(Quaternion.AngleAxis(180f, Vector3.left));
        rb.mass = 0.1f;
    }
}