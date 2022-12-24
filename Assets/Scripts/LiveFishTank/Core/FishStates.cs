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
    public virtual void OnCollidedWithWater() { }
    public virtual void OnCollidedWithTank() { }
    public virtual void OnCollidedWithTankDecor() { }
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

    public override void OnCollidedWithWater()
    {
        if (_rb.drag >= _maxDrag)
        {
            _rb.drag = 0f;
            _context.TogglePhysics(false);
            var angle = Random.Range(0f, 180f);

            PlaceablesManager.Instance.SavePlaceables();
            _context.State = new RotateTransitionalState(
                _context, Quaternion.AngleAxis(angle, Vector3.up), _rotateDuration, new WanderingAndFullState(_context));
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
        _startRotation = _rb.rotation;
        _endRotation = _startRotation * rotation;
        _duration = duration;
        _rotating = true;
    }

    public override void FixedUpdate()
    {
        if (_timeProgress > _duration)
            return;
        if (_duration > 0f)
        {
            _rb.rotation = Quaternion.Slerp(_startRotation, _endRotation, _timeProgress / _duration);
            _timeProgress += Time.fixedDeltaTime;
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
        if (_timeProgress > _duration)
        {
            _context.State = _nextState;
            return;
        }
        base.FixedUpdate();
        _timeProgress += Time.fixedDeltaTime;
    }
}

public abstract class WanderingState : RotatingState
{
    private float _movementPhase = 0f;
    private float _movementModifier;
    private float _timeBetweenRotations;
    private float _timeBeforeNextRotation;
    private bool _returningToCenter = false;

    public WanderingState(Fish context) : base(context, context.transform.rotation)
    {
        _rotating = false;
    }

    public override void FixedUpdate()
    {
        bool leavingWater = CheckLeavingWater();
        if (_returningToCenter)
        {
            if (leavingWater)
            {
                SwimForward();
                return;
            }
            else _returningToCenter = false;
        }
        else if (leavingWater)
        {
            _rb.rotation = _context.GetLookAt(_context.transform.parent.position, Vector3.up);
            return;
        }
        SetUpAndRotate();
        SwimForward();
    }

    public override void OnCollidedWithTank()
    {
        float angle;
        var waterExtent = _context.Tank.WaterCollider.bounds.extents.sqrMagnitude;
        var distanceFromTankCenter = _context.transform.localPosition.sqrMagnitude;
        if (distanceFromTankCenter / waterExtent > 0.75f)
        {
            var towardsTankCenter = Vector3.ProjectOnPlane(-_context.transform.localPosition, Vector3.up);
            angle = Vector3.SignedAngle(-_context.transform.right, towardsTankCenter, Vector3.up);
        }
        else angle = Random.value <= 0.5f ? Random.Range(-180f, -150f) : Random.Range(150f, 180f);
        BackUpAndTurnAway(angle);
    }

    public override void OnCollidedWithTankDecor()
    {
        var angle = Random.value <= 0.5f ? Random.Range(-120f, -60f) : Random.Range(60f, 120f);
        BackUpAndTurnAway(angle);
    }

    private void BackUpAndTurnAway(float turnAngle)
    {
        var backwardModifier = 0.3f;
        _rb.velocity = _context.transform.right * backwardModifier * _context.SwimSpeed;
        _context.State = new RotateTransitionalState(
            _context, Quaternion.AngleAxis(turnAngle, Vector3.up), 1f, GetCurrentStateWithSatiety());
    }

    private void SetUpAndRotate()
    {
        if (_rotating && _timeProgress < _duration)
            base.FixedUpdate();
        else if (_rotating && _timeProgress == _duration)
        {
            _rotating = false;
            _timeBeforeNextRotation = Random.Range(2f, 3f);
        }
        else
        {
            if (_timeBeforeNextRotation > 0f)
                _timeBeforeNextRotation -= Time.fixedDeltaTime;
            else
            {
                var rotate = Random.Range(0f, 10f);
                if (rotate > 7f)
                {
                    var angle = Random.Range(-120f, 120f);
                    _startRotation = _context.transform.rotation;
                    _endRotation = _startRotation * Quaternion.AngleAxis(angle, Vector3.up);
                    _duration = Random.Range(1f, 2.5f);
                    _timeProgress = 0f;
                    _rotating = true;
                }
            }
        }
    }

    private void SwimForward()
    {
        if (_movementPhase <= 0f)
        {
            var dice = Random.value;
            if (dice < 0.4f)
                _movementModifier = 0.2f;
            else
                _movementModifier = Random.Range(0.8f, 1f);

            _movementPhase = Random.Range(3f, 4f);
        }
        else _rb.velocity = -_context.transform.right * _movementModifier * _context.SwimSpeed;
        _movementPhase -= Time.fixedDeltaTime;
    }

    private bool CheckLeavingWater()
    {
        var waterHeightFromTankCenter = _context.Tank.WaterCollider.bounds.extents.y;
        var heightFromTankCenter = _context.transform.localPosition.y;
        return heightFromTankCenter / waterHeightFromTankCenter > 0.8f;
    }

    protected abstract WanderingState GetCurrentStateWithSatiety();
}

public class WanderingAndHungryState : WanderingState
{
    public WanderingAndHungryState(Fish context) : base(context) { }
    public override void Update()
    {
        var food = _context.DetectNextFood();
        if (food)
        {
            var directionToFood = food.transform.position - _context.transform.position;
            // food's behind decor, can't see can't reach so give up
            if (Physics.Raycast(_context.transform.position, directionToFood, 1000f, 1 << FishTank.DecorLayer))
            {
                _context.State = new WanderingAndHungryState(_context);
                return;
            }
            _context.State = new HeadingForFoodState(_context, food);
        }
        else if (!_context.CheckIsAlive())
            _context.State = new DeadState(_context);
    }

    protected override WanderingState GetCurrentStateWithSatiety()
    {
        return new WanderingAndHungryState(_context);
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

    protected override WanderingState GetCurrentStateWithSatiety()
    {
        return new WanderingAndFullState(_context);
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
        _endRotation = _context.GetLookAt(_food.transform.position, Vector3.up);
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
        rb.MoveRotation(Quaternion.AngleAxis(180f, Vector3.left));
        rb.constraints = RigidbodyConstraints.FreezeRotationZ;
        rb.mass = 0.1f;
    }
}