using UniRx;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    private CompositeDisposable subscriptions = new CompositeDisposable();

    public static GameEvents instance { get; private set; }
    public BoolReactiveProperty gameStarted { get; set; } = new BoolReactiveProperty(false);
    public BoolReactiveProperty gameWon { get; set; } = new BoolReactiveProperty(false);
    public BoolReactiveProperty gameLost { get; set; } = new BoolReactiveProperty(false);
    public BoolReactiveProperty canSlide { get; set; } = new BoolReactiveProperty(false);

    public IntReactiveProperty playerSize { get; set; } = new IntReactiveProperty(1);
    public IntReactiveProperty collectedWater { get; set; } = new IntReactiveProperty(0);
    public FloatReactiveProperty slidingDuration { get; set; } = new FloatReactiveProperty(0f);
    public FloatReactiveProperty maxSlidingDuration { get; set; } = new FloatReactiveProperty(0f);

    [Header("SFX")]
    [Space(4f)]
    public AudioClip sfxBirckBreaker;
    public AudioClip sfxFleshHit;
    public AudioClip sfxGameOver;
    public AudioClip sfxGameWon;
    public AudioClip sfxWaterCollected;

    private void Awake()
    {
        instance = this;
    }

    private void OnEnable()
    {
        playerSize.ObserveEveryValueChanged(x => x.Value)
            .Subscribe(value =>
            {
                if (value <= 0)
                    gameLost.SetValueAndForceNotify(true);
            })
            .AddTo(subscriptions);
    }
    private void OnDisable()
    {
        subscriptions.Clear();
    }
}