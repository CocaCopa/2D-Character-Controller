public enum HumanoidStateName {
    None,
    Idle,
    Run,
    LedgeGrabEnter,
    LedgeGrabLoop,
    LedgeClimb,
    DoubleJump,
    SlideLoop,
    Dash,
    WallSlide,
    TakeDamage,
    Death
};

public enum ChargeOverTime {
    ForceCancel,
    ForceRelease
};

public enum HitboxShape {
    Circle,
    Box
};

public enum PushMode {
    OnInitiate,
    OnRelease,
    Both
};
