using UniRx;
using UnityEngine;
using UniRx.Triggers;
using System.Collections;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    private CompositeDisposable subscriptions = new CompositeDisposable();

    [SerializeField] private float limitX;
    [SerializeField] private float sidewaySpeed;
    [SerializeField] private Transform playerModel;

    [Space(5f)]
    [Header("--Sliding Mechanic--")]
    [Space(1.5f)]
    [SerializeField] private float slidingSpeed = 6f;
    [SerializeField] private float slidingDecreasingRateMultiplier = 1;
    [SerializeField] private float maxSlidingTimer;
    public float slidingTimer;
    [SerializeField] private float watersToCollectForSliding = 2;

    private PathCreation.Examples.PathFollower pathFollowerScript;

    [SerializeField] private GameObject slidingTrail;
    [SerializeField] private Vector3 trailOffset;

    [SerializeField] private Slider waterMeterUi;
    [SerializeField] private GameObject sizeCounterUi;

    //---------------

    [SerializeField] private GameObject headGear; 

    private bool lockControls;
    private float _finalPos;
    private float _currentPos;

    private float defaultSpeed;

    private void Start()
    {
        //Debug.Log("Running code once");

        Observable.Timer(System.TimeSpan.FromSeconds(0))
            .Take(1)
            .Subscribe(_ =>
            {

                GameEvents.instance.maxSlidingDuration.Value = maxSlidingTimer;
                GameEvents.instance.slidingDuration.Value = GameEvents.instance.maxSlidingDuration.Value;

                waterMeterUi.maxValue = GameEvents.instance.maxSlidingDuration.Value;
                waterMeterUi.value = GameEvents.instance.slidingDuration.Value;

                waterMeterUi.gameObject.SetActive(false);
            })
            .AddTo(this);
    }

    private void OnEnable()
    {
        StartCoroutine(Subscribe());
        pathFollowerScript = GetComponent<PathCreation.Examples.PathFollower>();
        defaultSpeed = pathFollowerScript.speed;

    }

    private IEnumerator Subscribe()
    {
        yield return new WaitUntil(() => GameEvents.instance != null);
        this.UpdateAsObservable()
            .Where(_ => Input.GetMouseButton(0))
            .Subscribe(x =>
            {
                if (GameEvents.instance.gameStarted.Value && !GameEvents.instance.gameLost.Value
                && !GameEvents.instance.gameWon.Value)
                {
                    MovePlayer();
                }
            })
            .AddTo(subscriptions);

        GameEvents.instance.gameWon.ObserveEveryValueChanged(x => x.Value)
            .Subscribe(value =>
            {
                if (value)
                    lockControls = true;
            })
            .AddTo(subscriptions);

        GameEvents.instance.gameLost.ObserveEveryValueChanged(x => x.Value)
            .Subscribe(value =>
            {
                if (value)
                    lockControls = true;
            })
            .AddTo(subscriptions);

        GameEvents.instance.collectedWater.ObserveEveryValueChanged(x => x.Value)
            .Subscribe(value =>
            {
                if (value >= watersToCollectForSliding
                && GameEvents.instance.slidingDuration.Value >= 0)
                {
                    GameEvents.instance.canSlide.Value = true;
                    GameEvents.instance.collectedWater.Value = 0;
                }
            })
            .AddTo(subscriptions);

            this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                if (GameEvents.instance.canSlide.Value)
                {
                    pathFollowerScript.speed = slidingSpeed;

                    if (GameEvents.instance.slidingDuration.Value >= 0 
                    && !GameEvents.instance.gameLost.Value 
                    && !GameEvents.instance.gameWon.Value)
                    {
                        GameEvents.instance.slidingDuration.Value -= Time.deltaTime * slidingDecreasingRateMultiplier;
                        sizeCounterUi.SetActive(false);

                        waterMeterUi.gameObject.SetActive(true);
                        waterMeterUi.value = GameEvents.instance.slidingDuration.Value;

                        headGear.SetActive(false);

                        slidingTrail.SetActive(true);
                    }
                    else
                    {
                        GameEvents.instance.canSlide.Value = false;
                        waterMeterUi.gameObject.SetActive(false);
                        sizeCounterUi.SetActive(true);
                        headGear.SetActive(true);
                        slidingTrail.SetActive(false);

                    }
                }
                else
                {
                    pathFollowerScript.speed = defaultSpeed;
                    waterMeterUi.gameObject.SetActive(false);
                    sizeCounterUi.SetActive(true);
                    headGear.SetActive(true);
                    slidingTrail.SetActive(false);

                    GameEvents.instance.slidingDuration.Value = GameEvents.instance.maxSlidingDuration.Value;
                }

                //print(GameEvents.instance.slidingDuration.Value);
            })
            .AddTo(subscriptions);
        
    }

    private void OnDisable()
    {
        subscriptions.Clear();
    }

    private void MovePlayer()
    {
        if (Input.GetMouseButton(0))
        {
            float percentageX = (Input.mousePosition.x - Screen.width / 2) / (Screen.width * 0.5f) * 2;
            percentageX = Mathf.Clamp(percentageX, -1.0f, 1.0f);
            _finalPos = percentageX * limitX;
        }

        float delta = _finalPos - _currentPos;
        _currentPos += (delta * Time.deltaTime * sidewaySpeed);
        _currentPos = Mathf.Clamp(_currentPos, -limitX, limitX);
        playerModel.localPosition = new Vector3(0, _currentPos, 0);
    }
}