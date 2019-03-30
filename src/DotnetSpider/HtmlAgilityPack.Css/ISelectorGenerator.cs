namespace DotnetSpider.HtmlAgilityPack.Css
{
    /// <summary>
    /// Represent an implementation that is responsible for generating
    /// an implementation for a selector.
    /// </summary>
    public interface ISelectorGenerator
    {
        /// <summary>
        /// Delimits the initialization of a generation.
        /// </summary>
        void OnInit();

        /// <summary>
        /// Delimits the closing/conclusion of a generation.
        /// </summary>
        void OnClose();

        /// <summary>
        /// Delimits a selector generation in a group of selectors.
        /// </summary>
        void OnSelector();

        //
        // Selectors
        //

        /// <summary>
        /// Generates a <a href="http://www.w3.org/TR/css3-selectors/#type-selectors">type selector</a>,
        /// which represents an instance of the element type in the document tree. 
        /// </summary>
        void Type(NamespacePrefix prefix, string name);

        /// <summary>
        /// Generates a <a href="http://www.w3.org/TR/css3-selectors/#universal-selector">universal selector</a>,
        /// any single element in the document tree in any namespace 
        /// (including those without a namespace) if no default namespace 
        /// has been specified for selectors. 
        /// </summary>
        void Universal(NamespacePrefix prefix);

        /// <summary>
        /// Generates a <a href="http://www.w3.org/TR/css3-selectors/#Id-selectors">ID selector</a>,
        /// which represents an element instance that has an identifier that 
        /// matches the identifier in the ID selector.
        /// </summary>
        void Id(string id);

        /// <summary>
        /// Generates a <a href="http://www.w3.org/TR/css3-selectors/#class-html">class selector</a>,
        /// which is an alternative <see cref="AttributeIncludes"/> when 
        /// representing the <c>class</c> attribute. 
        /// </summary>
        void Class(string clazz);

        //
        // Attribute selectors
        //

        /// <summary>
        /// Generates an <a href="http://www.w3.org/TR/css3-selectors/#attribute-selectors">attribute selector</a>
        /// that represents an element with the given attribute <paramref name="name"/>
        /// whatever the values of the attribute.
        /// </summary>
        void AttributeExists(NamespacePrefix prefix, string name);

        /// <summary>
        /// Generates an <a href="http://www.w3.org/TR/css3-selectors/#attribute-selectors">attribute selector</a>
        /// that represents an element with the given attribute <paramref name="name"/>
        /// and whose value is exactly <paramref name="value"/>.
        /// </summary>
        void AttributeExact(NamespacePrefix prefix, string name, string value);

        /// <summary>
        /// Generates an <a href="http://www.w3.org/TR/css3-selectors/#attribute-selectors">attribute selector</a>
        /// that represents an element without the given attribute <paramref name="name"/>
        /// or with a different value <paramref name="value"/>.
        /// </summary>
        void AttributeNotEqual(NamespacePrefix prefix, string name, string value);
        
        /// <summary>
        /// Generates an <a href="http://www.w3.org/TR/css3-selectors/#attribute-selectors">attribute selector</a>
        /// that represents an element with the given attribute <paramref name="name"/>
        /// and whose value is a whitespace-separated list of words, one of 
        /// which is exactly <paramref name="value"/>.
        /// </summary>
        void AttributeIncludes(NamespacePrefix prefix, string name, string value);

        /// <summary>
        /// Generates an <a href="http://www.w3.org/TR/css3-selectors/#attribute-selectors">attribute selector</a>
        /// that represents an element whose the given attribute <paramref name="name"/>
        /// has a value that matches the specified regex.
        /// </summary>
        void AttributeRegexMatch(NamespacePrefix prefix, string name, string value);

        /// <summary>
        /// Generates an <a href="http://www.w3.org/TR/css3-selectors/#attribute-selectors">attribute selector</a>
        /// that represents an element with the given attribute <paramref name="name"/>,
        /// its value either being exactly <paramref name="value"/> or beginning 
        /// with <paramref name="value"/> immediately followed by "-" (U+002D).
        /// </summary>
        void AttributeDashMatch(NamespacePrefix prefix, string name, string value);

        /// <summary>
        /// Generates an <a href="http://www.w3.org/TR/css3-selectors/#attribute-selectors">attribute selector</a>
        /// that represents an element with the attribute <paramref name="name"/> 
        /// whose value begins with the prefix <paramref name="value"/>.
        /// </summary>
        void AttributePrefixMatch(NamespacePrefix prefix, string name, string value);

        /// <summary>
        /// Generates an <a href="http://www.w3.org/TR/css3-selectors/#attribute-selectors">attribute selector</a>
        /// that represents an element with the attribute <paramref name="name"/> 
        /// whose value ends with the suffix <paramref name="value"/>.
        /// </summary>
        void AttributeSuffixMatch(NamespacePrefix prefix, string name, string value);

        /// <summary>
        /// Generates an <a href="http://www.w3.org/TR/css3-selectors/#attribute-selectors">attribute selector</a>
        /// that represents an element with the attribute <paramref name="name"/> 
        /// whose value contains at least one instance of the substring <paramref name="value"/>.
        /// </summary>
        void AttributeSubstring(NamespacePrefix prefix, string name, string value);

        //
        // Pseudo-class selectors
        //

        /// <summary>
        /// Generates a <a href="http://www.w3.org/TR/css3-selectors/#pseudo-classes">pseudo-class selector</a>,
        /// which represents an element that is the first child of some other element.
        /// </summary>
        void FirstChild();

        /// <summary>
        /// Generates a <a href="http://www.w3.org/TR/css3-selectors/#pseudo-classes">pseudo-class selector</a>,
        /// which represents an element that is the last child of some other element.
        /// </summary>
        void LastChild();

        /// <summary>
        /// Generates a <a href="http://www.w3.org/TR/css3-selectors/#pseudo-classes">pseudo-class selector</a>,
        /// which represents an element that is the N-th child of some other element.
        /// </summary>
        void NthChild(int a, int b);

        /// <summary>
        /// Generates a <a href="http://www.w3.org/TR/css3-selectors/#pseudo-classes">pseudo-class selector</a>,
        /// which represents an element that has a parent element and whose parent 
        /// element has no other element children.
        /// </summary>
        void OnlyChild();

        /// <summary>
        /// Generates a <a href="http://www.w3.org/TR/css3-selectors/#pseudo-classes">pseudo-class selector</a>,
        /// which represents an element that has no children at all.
        /// </summary>
        void Empty();

        /// <summary>
        /// Generates a <a href="http://www.w3.org/TR/css3-selectors/#pseudo-classes">pseudo-class selector</a>,
        /// which represents the N-th matched element.
        /// </summary>
        void Eq(int n);

        /// <summary>
        /// Generates a <a href="http://www.w3.org/TR/css3-selectors/#pseudo-classes">pseudo-class selector</a>,
        /// which represents an element that contains an element that matches the query specified by the argument.
        /// </summary>
        void Has(ISelectorGenerator generator);

        /// <summary>
        /// Generates a <a href="http://www.w3.org/TR/css3-selectors/#pseudo-classes">pseudo-class selector</a>,
        /// that selects chunks of nodes based on the specified splitter selector (ignoring the content before the first separator)
        /// </summary>
        void SplitAfter(ISelectorGenerator subgenerator);

        /// <summary>
        /// Generates a <a href="http://www.w3.org/TR/css3-selectors/#pseudo-classes">pseudo-class selector</a>,
        /// that selects chunks of nodes based on the specified splitter selector (ignoring the content after the last separator)
        /// </summary>
        void SplitBefore(ISelectorGenerator subgenerator);

        /// <summary>
        /// Generates a <a href="http://www.w3.org/TR/css3-selectors/#pseudo-classes">pseudo-class selector</a>,
        /// that selects chunks of nodes based on the specified splitter selector (ignoring the content before the first separator and after the last separator)
        /// </summary>
        void SplitBetween(ISelectorGenerator subgenerator);

        /// <summary>
        /// Generates a <a href="http://www.w3.org/TR/css3-selectors/#pseudo-classes">pseudo-class selector</a>,
        /// that selects chunks of nodes based on the specified splitter selector (including the content before the first separator and after the last separator)
        /// </summary>
        void SplitAll(ISelectorGenerator subgenerator);

        /// <summary>
        /// Generates a <a href="http://www.w3.org/TR/css3-selectors/#pseudo-classes">pseudo-class selector</a>,
        /// that selects the nodes before a specified node, grouped into a single node
        /// </summary>
        void Before(ISelectorGenerator subgenerator);

        /// <summary>
        /// Generates a <a href="http://www.w3.org/TR/css3-selectors/#pseudo-classes">pseudo-class selector</a>,
        /// that selects the nodes after a specified node, grouped into a single node
        /// </summary>
        void After(ISelectorGenerator subgenerator);

        /// <summary>
        /// Generates a <a href="http://www.w3.org/TR/css3-selectors/#pseudo-classes">pseudo-class selector</a>,
        /// that selects the nodes between two specified nodes, grouped in a single node
        /// </summary>
        void Between(ISelectorGenerator startGenerator, ISelectorGenerator endGenerator);

        /// <summary>
        /// Generates a <a href="http://www.w3.org/TR/css3-selectors/#pseudo-classes">pseudo-class selector</a>,
        /// which represents an element that does not match the specified selector.
        /// </summary>
        void Not(ISelectorGenerator generator);


        /// <summary>
        /// Generates a <a href="http://www.w3.org/TR/css3-selectors/#pseudo-classes">pseudo-class selector</a>,
        /// which represents the parents of each matched element.
        /// </summary>
        void SelectParent();


        /// <summary>
        /// Generates a <a href="http://www.w3.org/TR/css3-selectors/#pseudo-classes">pseudo-class selector</a>,
        /// which represents an element that contains the specified text.
        /// </summary>
        void Contains(string text);


        /// <summary>
        /// Generates a <a href="http://www.w3.org/TR/css3-selectors/#pseudo-classes">pseudo-class selector</a>,
        /// which represents an element whose text content matches the specified regex.
        /// </summary>
        void Matches(string regex);

        /// <summary>
        /// Generates a <a href="http://www.w3.org/TR/css3-selectors/#pseudo-classes">pseudo-class selector</a>,
        /// which represents a user-defined selector.
        /// </summary>
        void CustomSelector(object selector);

        //
        // Combinators
        //

        /// <summary>
        /// Generates a <a href="http://www.w3.org/TR/css3-selectors/#combinators">combinator</a>,
        /// which represents a childhood relationship between two elements.
        /// </summary>
        void Child();

        /// <summary>
        /// Generates a <a href="http://www.w3.org/TR/css3-selectors/#combinators">combinator</a>,
        /// which represents a relationship between two elements where one element is an 
        /// arbitrary descendant of some ancestor element.
        /// </summary>
        void Descendant();

        /// <summary>
        /// Generates a <a href="http://www.w3.org/TR/css3-selectors/#combinators">combinator</a>,
        /// which represents elements that share the same parent in the document tree and 
        /// where the first element immediately precedes the second element.
        /// </summary>
        void Adjacent();

        /// <summary>
        /// Generates a <a href="http://www.w3.org/TR/css3-selectors/#combinators">combinator</a>,
        /// which separates two sequences of simple selectors. The elements represented
        /// by the two sequences share the same parent in the document tree and the
        /// element represented by the first sequence precedes (not necessarily
        /// immediately) the element represented by the second one.
        /// </summary>
        void GeneralSibling();

        /// <summary>
        /// Generates a <a href="http://www.w3.org/TR/css3-selectors/#pseudo-classes">pseudo-class selector</a>,
        /// which represents an element that is the N-th child from bottom up of some other element.
        /// </summary>
        void NthLastChild(int a, int b);

        /// <summary>
        /// Creates an empty instance of the same type of the current generator.
        /// </summary>
        ISelectorGenerator CreateNew();

        void AnchorToRoot();

        /// <summary>
        /// Generates a <a href="http://www.w3.org/TR/css3-selectors/#pseudo-classes">pseudo-class selector</a>,
        /// which represents the last matched element.
        /// </summary>
        void Last();


        object Selector { get; }
    }
}