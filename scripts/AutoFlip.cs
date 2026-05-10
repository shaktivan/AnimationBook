using UnityEngine;
using System.Collections;

public enum BookAnimationType
{
    Normal,
    Gentle,
    Fast,
    Heavy,
    Bounce,
    Custom
}

[RequireComponent(typeof(Book))]
public class AutoFlip : MonoBehaviour {
    public FlipMode Mode;
    public float PageFlipTime = 1;
    public float TimeBetweenPages = 1;
    public float DelayBeforeStarting = 0;
    public bool AutoStartFlip=true;
    public Book ControledBook;
    public int AnimationFramesCount = 40;

    [Header("Animation Type")]
    public BookAnimationType animationType = BookAnimationType.Normal;

    [Header("Flip Style")]
    public AnimationCurve flipCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [Range(0f, 3f)]
    public float curlHeight = 1f;
    [Range(0f, 1f)]
    public float pageLift = 0.5f;

    BookAnimationType _appliedType = (BookAnimationType)(-1);
    bool isFlipping = false;

    void Awake()
    {
        ControledBook = ControledBook != null ? ControledBook : GetComponent<Book>();
        Debug.Log($"[AutoFlip] Awake — AnimationType: {animationType} | PageFlipTime: {PageFlipTime}s | Frames: {AnimationFramesCount} | CurlHeight: {curlHeight} | PageLift: {pageLift}");
        Debug.Log($"[AutoFlip] AutoStartFlip: {AutoStartFlip} | Mode: {Mode} | TimeBetweenPages: {TimeBetweenPages}s | DelayBeforeStarting: {DelayBeforeStarting}s");
    }

    void OnValidate()
    {
        if (animationType == _appliedType) return;
        _appliedType = animationType;
        ApplyAnimationType();
    }

    void ApplyAnimationType()
    {
        switch (animationType)
        {
            case BookAnimationType.Normal:
                flipCurve  = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
                curlHeight = 1f;
                pageLift   = 0.5f;
                break;

            case BookAnimationType.Gentle:
                flipCurve  = AnimationCurve.Linear(0f, 0f, 1f, 1f);
                curlHeight = 0.6f;
                pageLift   = 0.2f;
                break;

            case BookAnimationType.Fast:
                // Ease-out: bursts off the edge quickly then decelerates
                flipCurve  = new AnimationCurve(
                    new Keyframe(0f, 0f, 0f, 4f),
                    new Keyframe(1f, 1f, 0f, 0f));
                curlHeight = 0.8f;
                pageLift   = 0.3f;
                break;

            case BookAnimationType.Heavy:
                // Ease-in: slow dramatic lift, then falls fast
                flipCurve  = new AnimationCurve(
                    new Keyframe(0f, 0f, 0f, 0f),
                    new Keyframe(1f, 1f, 4f, 0f));
                curlHeight = 1.8f;
                pageLift   = 0.7f;
                break;

            case BookAnimationType.Bounce:
                // Overshoots slightly then settles — gives a springy feel
                flipCurve  = new AnimationCurve(
                    new Keyframe(0f,    0f,    0f,   3f),
                    new Keyframe(0.65f, 1.05f, 2f,  -2f),
                    new Keyframe(0.82f, 0.97f, 1f,   1f),
                    new Keyframe(1f,    1f,    0f,   0f));
                curlHeight = 1.2f;
                pageLift   = 0.6f;
                break;

            case BookAnimationType.Custom:
                // Leave flipCurve / curlHeight / pageLift untouched
                break;
        }
    }

    void Start () {
        if (!ControledBook)
            ControledBook = GetComponent<Book>();
        ApplyAnimationType();
        if (AutoStartFlip)
            StartFlipping();
        ControledBook.OnFlip.AddListener(new UnityEngine.Events.UnityAction(PageFlipped));
	}
    void PageFlipped()
    {
        isFlipping = false;
    }
	public void StartFlipping()
    {
        StartCoroutine(FlipToEnd());
    }
    public void FlipRightPage()
    {
        if (isFlipping) return;
        if (ControledBook.currentPage >= ControledBook.TotalPageCount) return;
        isFlipping = true;
        float frameTime = PageFlipTime / AnimationFramesCount;
        float xc = (ControledBook.EndBottomRight.x + ControledBook.EndBottomLeft.x) / 2;
        float xl = ((ControledBook.EndBottomRight.x - ControledBook.EndBottomLeft.x) / 2) * 0.9f;
        float h = Mathf.Abs(ControledBook.EndBottomRight.y) * 0.9f;
        StartCoroutine(FlipRTL(xc, xl, h, frameTime));
    }
    public void FlipLeftPage()
    {
        if (isFlipping) return;
        if (ControledBook.currentPage <= 0) return;
        isFlipping = true;
        float frameTime = PageFlipTime / AnimationFramesCount;
        float xc = (ControledBook.EndBottomRight.x + ControledBook.EndBottomLeft.x) / 2;
        float xl = ((ControledBook.EndBottomRight.x - ControledBook.EndBottomLeft.x) / 2) * 0.9f;
        float h = Mathf.Abs(ControledBook.EndBottomRight.y) * 0.9f;
        StartCoroutine(FlipLTR(xc, xl, h, frameTime));
    }
    IEnumerator FlipToEnd()
    {
        yield return new WaitForSeconds(DelayBeforeStarting);
        float frameTime = PageFlipTime / AnimationFramesCount;
        float xc = (ControledBook.EndBottomRight.x + ControledBook.EndBottomLeft.x) / 2;
        float xl = ((ControledBook.EndBottomRight.x - ControledBook.EndBottomLeft.x) / 2)*0.9f;
        float h = Mathf.Abs(ControledBook.EndBottomRight.y)*0.9f;
        switch (Mode)
        {
            case FlipMode.RightToLeft:
                while (ControledBook.currentPage < ControledBook.TotalPageCount)
                {
                    StartCoroutine(FlipRTL(xc, xl, h, frameTime));
                    yield return new WaitForSeconds(TimeBetweenPages);
                }
                break;
            case FlipMode.LeftToRight:
                while (ControledBook.currentPage > 0)
                {
                    StartCoroutine(FlipLTR(xc, xl, h, frameTime));
                    yield return new WaitForSeconds(TimeBetweenPages);
                }
                break;
        }
    }
    IEnumerator FlipRTL(float xc, float xl, float h, float frameTime)
    {
        float yBottom = -h;
        float yPeak = Mathf.Min(h * (curlHeight * (1f + pageLift) - 1f), h);

        float x = xc + xl;
        ControledBook.DragRightPageToPoint(new Vector3(x, yBottom, 0));
        for (int i = 0; i < AnimationFramesCount; i++)
        {
            float t = flipCurve.Evaluate((float)i / AnimationFramesCount);
            x = xc + xl - t * xl * 2f;
            float y = Mathf.Lerp(yBottom, yPeak, Mathf.Sin(t * Mathf.PI));
            ControledBook.UpdateBookRTLToPoint(new Vector3(x, y, 0));
            yield return new WaitForSeconds(frameTime);
        }
        ControledBook.ReleasePage();
    }
    IEnumerator FlipLTR(float xc, float xl, float h, float frameTime)
    {
        float yBottom = -h;
        float yPeak = Mathf.Min(h * (curlHeight * (1f + pageLift) - 1f), h);

        float x = xc - xl;
        ControledBook.DragLeftPageToPoint(new Vector3(x, yBottom, 0));
        for (int i = 0; i < AnimationFramesCount; i++)
        {
            float t = flipCurve.Evaluate((float)i / AnimationFramesCount);
            x = xc - xl + t * xl * 2f;
            float y = Mathf.Lerp(yBottom, yPeak, Mathf.Sin(t * Mathf.PI));
            ControledBook.UpdateBookLTRToPoint(new Vector3(x, y, 0));
            yield return new WaitForSeconds(frameTime);
        }
        ControledBook.ReleasePage();
    }
}
