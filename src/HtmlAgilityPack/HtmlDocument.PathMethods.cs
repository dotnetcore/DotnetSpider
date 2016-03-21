using System;
using System.IO;
using System.Text;

namespace HtmlAgilityPack
{
    public partial class HtmlDocument
    {
        /// <summary>
        /// Detects the encoding of an HTML document from a file first, and then loads the file.
        /// </summary>
        /// <param name="path">The complete file path to be read.</param>
        public void DetectEncodingAndLoad(string path)
        {
            DetectEncodingAndLoad(path, true);
        }

        /// <summary>
        /// Detects the encoding of an HTML document from a file first, and then loads the file.
        /// </summary>
        /// <param name="path">The complete file path to be read. May not be null.</param>
        /// <param name="detectEncoding">true to detect encoding, false otherwise.</param>
        public void DetectEncodingAndLoad(string path, bool detectEncoding)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            Encoding enc;
            if (detectEncoding)
            {
                enc = DetectEncoding(path);
            }
            else
            {
                enc = null;
            }

            if (enc == null)
            {
                Load(path);
            }
            else
            {
                Load(path, enc);
            }
        }
        /// <summary>
        /// Detects the encoding of an HTML file.
        /// </summary>
        /// <param name="path">Path for the file containing the HTML document to detect. May not be null.</param>
        /// <returns>The detected encoding.</returns>
        public Encoding DetectEncoding(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            using (StreamReader sr = new StreamReader(File.OpenRead(path), OptionDefaultStreamEncoding))
            {
                Encoding encoding = DetectEncoding(sr);
                return encoding;
            }
        }

        /// <summary>
        /// Loads an HTML document from a file.
        /// </summary>
        /// <param name="path">The complete file path to be read. May not be null.</param>
        public void Load(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            using(StreamReader sr = new StreamReader(File.OpenRead(path), OptionDefaultStreamEncoding))
            {
                Load(sr);
            }
        }

        /// <summary>
        /// Loads an HTML document from a file.
        /// </summary>
        /// <param name="path">The complete file path to be read. May not be null.</param>
        /// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the file.</param>
        public void Load(string path, bool detectEncodingFromByteOrderMarks)
        {
            if (path == null)
                throw new ArgumentNullException("path");

           using(StreamReader sr = new StreamReader(File.OpenRead(path), detectEncodingFromByteOrderMarks))
           {
               Load(sr);
           }
        }

        /// <summary>
        /// Loads an HTML document from a file.
        /// </summary>
        /// <param name="path">The complete file path to be read. May not be null.</param>
        /// <param name="encoding">The character encoding to use. May not be null.</param>
        public void Load(string path, Encoding encoding)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            if (encoding == null)
                throw new ArgumentNullException("encoding");

            using(StreamReader sr = new StreamReader(File.OpenRead(path), encoding))
            {
                Load(sr);
            }
        }

        /// <summary>
        /// Loads an HTML document from a file.
        /// </summary>
        /// <param name="path">The complete file path to be read. May not be null.</param>
        /// <param name="encoding">The character encoding to use. May not be null.</param>
        /// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the file.</param>
        public void Load(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            if (encoding == null)
                throw new ArgumentNullException("encoding");

           using(StreamReader sr = new StreamReader(File.OpenRead(path), encoding, detectEncodingFromByteOrderMarks))
           {
               Load(sr);
           }
        }

        /// <summary>
        /// Loads an HTML document from a file.
        /// </summary>
        /// <param name="path">The complete file path to be read. May not be null.</param>
        /// <param name="encoding">The character encoding to use. May not be null.</param>
        /// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the file.</param>
        /// <param name="buffersize">The minimum buffer size.</param>
        public void Load(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks, int buffersize)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            if (encoding == null)
                throw new ArgumentNullException("encoding");

           using(StreamReader sr = new StreamReader(File.OpenRead(path), encoding, detectEncodingFromByteOrderMarks, buffersize))
           {
               Load(sr);
           }
        }
        /// <summary>
        /// Saves the mixed document to the specified file.
        /// </summary>
        /// <param name="filename">The location of the file where you want to save the document.</param>
        public void Save(string filename)
        {
            using(StreamWriter sw = new StreamWriter(File.OpenWrite(filename), GetOutEncoding()))
            {
                Save(sw);
            }
        }

        /// <summary>
        /// Saves the mixed document to the specified file.
        /// </summary>
        /// <param name="filename">The location of the file where you want to save the document. May not be null.</param>
        /// <param name="encoding">The character encoding to use. May not be null.</param>
        public void Save(string filename, Encoding encoding)
        {
            if (filename == null)
            {
                throw new ArgumentNullException("filename");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            using(StreamWriter sw = new StreamWriter(File.OpenWrite(filename), encoding))
            {
                Save(sw);
            }
        }

    }
}
