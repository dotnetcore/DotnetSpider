namespace HtmlAgilityPack.Css
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a selector implementation over an arbitrary type of elements.
    /// </summary>
    public delegate IEnumerable<TElement> Selector<TElement>(IEnumerable<TElement> elements);
}