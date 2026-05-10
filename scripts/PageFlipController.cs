using UnityEngine;

[RequireComponent(typeof(Book))]
public class PageFlipController : MonoBehaviour
{
    [Header("Flip Settings")]
    public FlipMode flipMode = FlipMode.RightToLeft;

    [Range(0f, 1f)]
    [Tooltip("Drag this slider to control the page blend. 0 = page at rest, 1 = page fully flipped.")]
    public float pageBlend = 0f;

    Book book;
    float prevBlend = 0f;
    bool dragActive = false;

    void Awake()
    {
        book = GetComponent<Book>();
    }

    void Update()
    {
        if (Mathf.Approximately(pageBlend, prevBlend)) return;

        float xc = (book.EndBottomRight.x + book.EndBottomLeft.x) * 0.5f;
        float xl = ((book.EndBottomRight.x - book.EndBottomLeft.x) * 0.5f) * 0.9f;
        float h  = Mathf.Abs(book.EndBottomRight.y) * 0.9f;

        if (xl < 0.001f) return;

        float x = flipMode == FlipMode.RightToLeft
            ? xc + xl * (1f - 2f * pageBlend)
            : xc - xl * (1f - 2f * pageBlend);
        float y = (-h / (xl * xl)) * (x - xc) * (x - xc);
        Vector3 point = new Vector3(x, y, 0f);

        if (!dragActive && pageBlend > 0f)
        {
            bool canFlip = flipMode == FlipMode.RightToLeft
                ? book.currentPage < book.TotalPageCount
                : book.currentPage > 0;

            if (canFlip)
            {
                dragActive = true;
                if (flipMode == FlipMode.RightToLeft)
                    book.DragRightPageToPoint(point);
                else
                    book.DragLeftPageToPoint(point);
            }
        }

        if (dragActive)
        {
            if (flipMode == FlipMode.RightToLeft)
                book.UpdateBookRTLToPoint(point);
            else
                book.UpdateBookLTRToPoint(point);

            if (pageBlend >= 1f || pageBlend <= 0f)
            {
                book.ReleasePage();
                dragActive = false;
                pageBlend = 0f;
                prevBlend = 0f;
                return;
            }
        }

        prevBlend = pageBlend;
    }
}
