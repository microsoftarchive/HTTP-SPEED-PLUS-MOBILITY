//-----------------------------------------------------------------------
// <copyright file="XHTMLDocument.cs" company="Microsoft Open Technologies, Inc.">
//
// Copyright 2012 Microsoft Open Technologies, Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
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
