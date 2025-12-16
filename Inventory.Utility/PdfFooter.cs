using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Inventory.Utility
{
    public class PdfFooter : PdfPageEventHelper
    {
        private PdfTemplate _totalPagesTemplate;
        private BaseFont _baseFont;
        private readonly string _generatedAt;

        public PdfFooter(string generatedAt)
        {
            _generatedAt = generatedAt;
        }

        public override void OnOpenDocument(PdfWriter writer, Document document)
        {
            _totalPagesTemplate = writer.DirectContent.CreateTemplate(50, 50);
            _baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);
        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            float y = document.BottomMargin - 10;

            // ---- Page X of ----
            string pageText = $"Page {writer.PageNumber} of ";
            writer.DirectContent.BeginText();
            writer.DirectContent.SetFontAndSize(_baseFont, 8);
            writer.DirectContent.SetTextMatrix(document.LeftMargin, y);
            writer.DirectContent.ShowText(pageText);
            writer.DirectContent.EndText();

            // ---- Total pages placeholder (Linux safe) ----
            writer.DirectContent.AddTemplate(
                _totalPagesTemplate,
                1, 0, 0, 1,
                document.LeftMargin + _baseFont.GetWidthPoint(pageText, 8),
                y
            );

            // ---- Right side timestamp ----
            string rightText = $"Report Generated at: {_generatedAt}";
            float rightWidth = _baseFont.GetWidthPoint(rightText, 8);

            writer.DirectContent.BeginText();
            writer.DirectContent.SetFontAndSize(_baseFont, 8);
            writer.DirectContent.SetTextMatrix(
                document.PageSize.Width - document.RightMargin - rightWidth,
                y
            );
            writer.DirectContent.ShowText(rightText);
            writer.DirectContent.EndText();
        }

        public override void OnCloseDocument(PdfWriter writer, Document document)
        {
            _totalPagesTemplate.BeginText();
            _totalPagesTemplate.SetFontAndSize(_baseFont, 8);
            _totalPagesTemplate.SetTextMatrix(0, 0);
            _totalPagesTemplate.ShowText(writer.PageNumber.ToString());
            _totalPagesTemplate.EndText();
        }
    }
}