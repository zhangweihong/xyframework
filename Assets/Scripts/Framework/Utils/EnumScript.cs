/// <summary>
/// ui的id 只是在编辑器使用
/// </summary>
public enum UIID
{
    None = -1,
    Login,
    Game,
    User,
    Command,
    Story,
    Lab,
    Compose,
    Bot,
    RemoteCtr,
    Joy,
    Joy2,
    Speech,
    Gyroscope,
    Draw,
    NewUser,
    Scene
}

/// <summary>
/// ui动画 lua 和 C# 通用
/// </summary>
[XLua.GCOptimize]
public enum UIAnimationState
{
    Fade = 0,
    Move
}
/// <summary>
/// asset的类型 lua 和 C# 通用
/// </summary>
[XLua.GCOptimize]
public enum AssetType
{
    None = 0,
    Texture,
    GameObjct,
    AudioClip,
    Mesh,
    AnimationClip,
    Material,
    Shader,
    File,
    Data
}

/// <summary>
/// 按钮的点击动画
/// </summary>
[XLua.GCOptimize]
public enum ButtonAniType
{
    None = -1,
    Scale = 0
}