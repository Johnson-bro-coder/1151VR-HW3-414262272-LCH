using UnityEngine;

/// <summary>
/// 輪盤控制器 (RouletteController)
/// 實作功能：
/// 1. 點擊螢幕或觸控開始旋轉
/// 2. 加速旋轉至最大速度後，自然減速直至平滑停止
/// 3. 旋轉時播放指針跳格音效，停止時播放結算音效
/// </summary>
public class RouletteController : MonoBehaviour
{
    [Header("旋轉參數設定")]
    [Tooltip("當前旋轉速度")]
    public float rotSpeed = 0f;

    [Tooltip("加速階段的加速度 (度/秒)")]
    public float acceleration = 25f;

    [Tooltip("最高旋轉速度範圍")]
    public float minMaxSpeed = 16f;
    public float maxMaxSpeed = 24f;

    [Tooltip("減速阻尼衰減係數 (越接近 1 減速越慢)")]
    [Range(0.95f, 0.995f)]
    public float damping = 0.985f;

    [Header("音效設定")]
    public AudioSource audioSource;
    public AudioClip tickSound;
    public AudioClip winSound;

    // 狀態變數
    private bool isSpinning = false;
    private bool isAccelerating = false;
    private float targetMaxSpeed = 0f;
    private float lastAngle = 0f;

    void Start()
    {
        // 若未在 Inspector 指定 AudioSource，則自動取得身上的組件
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        lastAngle = transform.eulerAngles.z;
    }

    void Update()
    {
        // 1. 偵測玩家輸入：支援電腦滑鼠左鍵點擊與手機螢幕單指觸碰
        bool isInputTriggered = Input.GetMouseButtonDown(0) || 
                               (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);

        // 如果目前沒有在旋轉，且玩家點擊，則啟動加速旋轉
        if (isInputTriggered && !isSpinning)
        {
            StartSpin();
        }

        // 2. 處理旋轉與物理速度演算法 (加速 -> 減速 -> 停止)
        if (isSpinning)
        {
            UpdateSpinPhysics();
            CheckAndPlaySound();
        }
    }

    /// <summary>
    /// 開始啟動輪盤
    /// </summary>
    void StartSpin()
    {
        isSpinning = true;
        isAccelerating = true;
        rotSpeed = 2f; // 給予微小啟動初速
        // 每次抽獎隨機產生最大速度，確保每次停在不同獎項
        targetMaxSpeed = Random.Range(minMaxSpeed, maxMaxSpeed);
    }

    /// <summary>
    /// 核心旋轉邏輯：先加速後減速
    /// </summary>
    void UpdateSpinPhysics()
    {
        if (isAccelerating)
        {
            // 加速階段：平滑增加旋轉速度
            rotSpeed += acceleration * Time.deltaTime;

            if (rotSpeed >= targetMaxSpeed)
            {
                rotSpeed = targetMaxSpeed;
                isAccelerating = false; // 達到最高速，切換為減速階段
            }
        }
        else
        {
            // 減速階段：使用阻尼係數逐漸衰減
            rotSpeed *= damping;

            // 當速度低於門檻時完全停止
            if (rotSpeed < 0.05f)
            {
                rotSpeed = 0f;
                isSpinning = false;
                OnSpinFinished();
            }
        }

        // 旋轉輪盤物件 (順時針旋轉為負 Z 軸)
        transform.Rotate(0, 0, -rotSpeed);
    }

    /// <summary>
    /// 依據旋轉角度播放指針跳動音效 (每 45 度為一格)
    /// </summary>
    void CheckAndPlaySound()
    {
        float currentAngle = transform.eulerAngles.z;

        // 計算是否越過 45 度分界線 (360 / 8 = 45 度)
        int currentSector = Mathf.FloorToInt(currentAngle / 45f);
        int lastSector = Mathf.FloorToInt(lastAngle / 45f);

        if (currentSector != lastSector)
        {
            if (audioSource != null && tickSound != null)
            {
                // 音調可隨速度微調增加生動感
                audioSource.pitch = Mathf.Clamp(rotSpeed / 12f, 0.8f, 1.4f);
                audioSource.PlayOneShot(tickSound, 0.7f);
            }
        }

        lastAngle = currentAngle;
    }

    /// <summary>
    /// 輪盤完全停止時觸發
    /// </summary>
    void OnSpinFinished()
    {
        if (audioSource != null && winSound != null)
        {
            audioSource.pitch = 1.0f;
            audioSource.PlayOneShot(winSound, 1.0f);
        }
        Debug.Log($"【輪盤停止】最終停止角度：{transform.eulerAngles.z:F1} 度");
    }
}
