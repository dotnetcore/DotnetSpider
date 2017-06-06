// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>
namespace DotnetSpider.HtmlAgilityPack
{
    /// <summary>
    /// Represents a parsing error found during document parsing.
    /// </summary>
    public class HtmlParseError
    {
        #region Fields

        private HtmlParseErrorCode _code;
        private int _line;
        private int _linePosition;
        private string _reason;
        private string _sourceText;
        private int _streamPosition;

        #endregion

        #region Constructors

        internal HtmlParseError(
            HtmlParseErrorCode code,
            int line,
            int linePosition,
            int streamPosition,
            string sourceText,
            string reason)
        {
            _code = code;
            _line = line;
            _linePosition = linePosition;
            _streamPosition = streamPosition;
            _sourceText = sourceText;
            _reason = reason;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the type of error.
        /// </summary>
        public HtmlParseErrorCode Code => _code;

        /// <summary>
        /// Gets the line number of this error in the document.
        /// </summary>
        public int Line => _line;

        /// <summary>
        /// Gets the column number of this error in the document.
        /// </summary>
        public int LinePosition => _linePosition;

        /// <summary>
        /// Gets a description for the error.
        /// </summary>
        public string Reason => _reason;

        /// <summary>
        /// Gets the the full text of the line containing the error.
        /// </summary>
        public string SourceText => _sourceText;

        /// <summary>
        /// Gets the absolute stream position of this error in the document, relative to the start of the document.
        /// </summary>
        public int StreamPosition => _streamPosition;

        #endregion
    }
}