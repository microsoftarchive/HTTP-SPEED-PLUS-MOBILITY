//-----------------------------------------------------------------------
// <copyright file="XHTMLDocument.cs" company="Microsoft Open Technologies, Inc.">
//
// ---------------------------------------
// HTTPbis
// Copyright Microsoft Open Technologies, Inc.
// ---------------------------------------
// Microsoft Reference Source License.
// 
// This license governs use of the accompanying software. 
// If you use the software, you accept this license. 
// If you do not accept the license, do not use the software.
// 
// 1. Definitions
// 
// The terms "reproduce," "reproduction," and "distribution" have the same meaning here 
// as under U.S. copyright law.
// "You" means the licensee of the software.
// "Your company" means the company you worked for when you downloaded the software.
// "Reference use" means use of the software within your company as a reference, in read // only form, 
// for the sole purposes of debugging your products, maintaining your products, 
// or enhancing the interoperability of your products with the software, 
// and specifically excludes the right to distribute the software outside of your company.
// "Licensed patents" means any Licensor patent claims which read directly on the software 
// as distributed by the Licensor under this license. 
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, the Licensor grants you a non-transferable, 
// non-exclusive, worldwide, royalty-free copyright license to reproduce the software for reference use.
// (B) Patent Grant- Subject to the terms of this license, the Licensor grants you a non-transferable, 
// non-exclusive, worldwide, royalty-free patent license under licensed patents for reference use. 
// 
// 3. Limitations
// (A) No Trademark License- This license does not grant you any rights 
// to use the Licensor’s name, logo, or trademarks.
// (B) If you begin patent litigation against the Licensor over patents that you think may apply 
// to the software (including a cross-claim or counterclaim in a lawsuit), your license 
// to the software ends automatically. 
// (C) The software is licensed "as-is." You bear the risk of using it. 
// The Licensor gives no express warranties, guarantees or conditions. 
// You may have additional consumer rights under your local laws 
// which this license cannot change. To the extent permitted under your local laws, 
// the Licensor excludes the implied warranties of merchantability, 
// fitness for a particular purpose and non-infringement. 
// 
// -----------------End of License---------
//
// </copyright>
//-----------------------------------------------------------------------
namespace Client
{
    using System.Linq;
    using System.Xml.Linq;

    /// <summary>
    /// HTML parser class
    /// </summary>
    public class XHtmlDocument
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="XHtmlDocument"/> class.
        /// </summary>
        /// <param name="doc">the document.</param>
        private XHtmlDocument(XDocument doc)
        {
            this.Document = doc;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets image name list.
        /// </summary>
        public string[] Images { get; private set; }

        /// <summary>
        /// Gets links name list.
        /// </summary>
        public string[] Links { get; private set; }

        /// <summary>
        /// Gets or sets scripts name list.
        /// </summary>
        public string[] Scripts { get; set; }

        /// <summary>
        /// Gets document.
        /// </summary>
        public XDocument Document { get; private set; }

        #endregion

        /// <summary>
        /// Document parser.
        /// </summary>
        /// <param name="content">The document to parse.</param>
        /// <returns>The parsed document.</returns>
        public static XHtmlDocument Parse(string content)
        {
            XDocument doc = XDocument.Parse(content.Trim(new[] { ' ', '\uFEFF', '\r', '\n' }));
            XHtmlDocument htmDoc = new XHtmlDocument(doc);
            if (doc.Root != null)
            {
                XNamespace ns = "http://www.w3.org/1999/xhtml";
                htmDoc.Images = (from img in doc.Root.Descendants(ns + "img")
                                    let xAttribute = img.Attribute("src")
                                    where xAttribute != null
                                    select xAttribute.Value).Distinct().ToArray();
                htmDoc.Links = (from linc in doc.Root.Descendants(ns + "link")
                                let xAttribute = linc.Attribute("href")
                                where xAttribute != null
                                select xAttribute.Value).Distinct().ToArray();
                htmDoc.Scripts = (from script in doc.Root.Descendants(ns + "script")
                                let xAttribute = script.Attribute("src")
                                where xAttribute != null
                                select xAttribute.Value).Distinct().ToArray();
            }

            return htmDoc;
        }
    }
}
