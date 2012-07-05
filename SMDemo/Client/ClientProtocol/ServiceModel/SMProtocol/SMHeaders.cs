//-----------------------------------------------------------------------
// <copyright file="SMHeaders.cs" company="Microsoft Open Technologies, Inc.">
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
namespace System.ServiceModel.SMProtocol
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// Stream headers collection.
    /// </summary>
    [Serializable]
    public class SMHeaders : Dictionary<string, string>
    {
        #region Public Constants
        /// <summary>
        /// Path HTTP header constant
        /// </summary>   
        public const string Path = ":path";

        /// <summary>
        /// Method HTTP header constant
        /// </summary>   
        public const string Method = ":method";

        /// <summary>
        /// Version HTTP header constant
        /// </summary>   
        public const string Version = ":version";

        /// <summary>
        /// Host HTTP header constant
        /// </summary>   
        public const string Host = ":host";

        /// <summary>
        /// Scheme HTTP header constant
        /// </summary>   
        public const string Scheme = ":scheme";

        /// <summary>
        /// Status HTTP header constant
        /// </summary>   
        public const string Status = ":status";

        /// <summary>
        /// User-Agent HTTP header constant
        /// </summary>   
        public const string UserAgent = ":user-agent";

        /// <summary>
        /// Accept-encoding HTTP header constant
        /// </summary>   
        public const string AcceptEncoding = ":accept-encoding";

        /// <summary>
        /// Content-type HTTP header constant
        /// </summary>   
        public const string ContentType = ":content-type";

        /// <summary>
        /// Cache-control HTTP header constant
        /// </summary>
        public const string CacheControl = ":cache-control";

        /// <summary>
        /// Cookie HTTP header constant
        /// </summary>
        public const string Cookie = ":cookie";


        #endregion

        #region Private Constants

        /// <summary>
        /// Valid HTTP cache control strings
        /// </summary>
        private readonly string[] CacheControlValues = new string[] { 
            "no-cache",
            "no-store",
            "max-age=1250",
            "max-stale=99999",
            "min-fresh=8304",
            "no-transform",
            "only-if-cached",
            "public",
            "private",
            "must-revalidate",
            "proxy-revalidate",
            "s-maxage=56",
            "community=\"S+M protocol demo communication group\"",
            "company=\"Microsoft OpenTech\""
        };

        private readonly string[] ClientCookies = new string[] {
            "dtApps=|AA.com|1; dtCookie=864CDCDC97D71A60647C8FC92010A5DA; v1st=3AD3BDCEFCF7C829; JSESSIONID=0000Yt1WJcAQl4mhZfL--E0lIQ0:15lkmui9l",
            "CH=AgBP13UgAA+CIAAmFCAADyIgAAeSIAAR5yAAMhAgACx3IAArYiAALuAgACij; MSC=t=1339520435X",
            "CH=AgBP13UgAA++FGGmFCAADyIgAAeSIAARBNMMshAgFFKDRRSrYiAALuAgACij; MSC=t=1339535355X",
            "MS0=473c21476f16441da07be03d244b8c33; MC1=GUID=1b068fd69499534ca1d2d31052b33bde&HASH=d68f&LV=20126&V=3; MS_WT=ta_Office={\"Value\":\"{\"_wt.control-327131-ta_Office\":{\"value\":\"{\\\"runid\\\":\\\"588172\\\",\\\"testid\\\":\\\"588164\\\",\\\"throttleR",
            "ebay=%5Esbf%3D%2320000000000000000000000%5Ecv%3D15555%5Ejs%3D0%5E; dp1=bu1p/QEBfX0BAX19AQA**51b8a938^pbf/%2380000000000051b8a938^; cssg=e1a3c24f1370a0b2a535fe85fff019f5; s=CgAD4ACBP2Mc4ZTFhM2MyNGYxMzcwYTBiMmE1MzVmZTg1ZmZmMDE5ZjUA7gBDT9jHODMGaHR0cDovL",
            "ebay=%5Esbf%3D%23%5E; dp1=bpbf/%2380000000000051b8a92e^u1p/QEBfX0BAX19AQA**51b8a92e^; cssg=e1a3afec1370a56125f7bf67ffccb081; s=CgAD4ACBP2McuZTFhM2FmZWMxMzcwYTU2MTI1ZjdiZjY3ZmZjY2IwODEqptpe; nonsession=CgADKACBZPXcuZTFhM2FmZGQxMzcwYTU2MTI1ZjdiZjY3ZmZj",
            "ebay=%5Ecv%3D15555%5Esbf%3D%23%5E; dp1=bu1p/QEBfX0BAX19AQA**51b8a93c^pbf/%2380000000000451b8a93c^; cssg=e1a3afec1370a56125f7bf67ffccb081; s=BAQAAATfOiknyAAWAAPgAIE/YxzxlMWEzYWZlYzEzNzBhNTYxMjVmN2JmNjdmZmNjYjA4MQASAApP2Mc8dGVzdENvb2tpZQOcngr0EcRJBzaeR",
            "ebay=%5Ecv%3D15555%5Ejs%3D0%5Esbf%3D%2320000000000000000000000%5E; dp1=bpbf/%2380004000000451b8a94d^u1p/QEBfX0BAX19AQA**51b8a94d^; cssg=e1a3afec1370a56125f7bf67ffccb081; s=BAQAAATfOiknyAAWAAPgAIE/Yx01lMWEzYWZlYzEzNzBhNTYxMjVmN2JmNjdmZmNjYjA4MQDuAENP2",
            "ebay=%5Esbe%3D%2320000000000000000000000%5Ecv%3D15555%5Ejs%3D0%5E; dp1=bu1p/QEBfX0BAX19AQA**51b8a941^pbf/%2380000000000451b8a941^; cssg=e1a3afec1370a56125f7bf67ffccb081; s=BAQAAATfOiknyAAWAAPgAIE/Yx0FlMWEzYWZlYzEzNzBhNTYxMjVmN2JmNjdmZmNjYjA4MQASAApP2",
            "_HOP=I=1&TS=1339520431; MC1=V=3&GUID=bbc9d7924e954f81986146f0d70454ce; mh=MSFT; CC=US; CULTURE=EN-US; expid=id=433be17e662a4c91ba8a497b322420ef&bd=2012-06-12T17:00:30.080&v=2",
            "TLTSID=26DD6DC8B4B010B40061D16ADEA2DF0A; TLTUID=26DD6DC8B4B010B40061D16ADEA2DF0A; tyrg1st=C9D186E8D4B69A05; JSID=43ADC4F4D2D5082B4800B6D6ABD7B371.p0729; PBXID=p0729; TUID=097d93cc-d198-42a5-8a77-010d29d2ee3e; Service=TRAVELOCITY; pcookie=n; TVLY_GEO=",
            "TLTSID=26DDFFC8B4B010B40061D16ADEA2DF0A; TLTUID=26DD6DC8B4B010B40061D16ADEA2DF0A; tyrg1st=C9D186E8D4B69A05; JSID=43ADC4F4D2D5082B4800B6D6ABD7B371.p0729; PBXID=p0729; TUID=097d93cc-d198-42a5-8a77-010d29d2ee3e; Service=TRAVELOCITY; pcookie=n",
            "TLTSID=26DD6DC8B4B010B40061D16ADE88DF0A; TLTUID=26D56DC8B4B010B40061D16ADEA2DF0A; tyrg1st=C9D1FAE8D4B69A05; JSID=43ADC4F4D2D5082B48F446D6ABD7B371.p0729; PBXID=p0729; TUID=097d93cc-d198-42a5-8a77-010d29d2ee3e; Service=TRAVELOCITY; pcookie=n",
            "TLTHID=3E04819EB4B010B47903C3C9188D5D79; TLTSID=3E04819EB4B010B47903C3C9188D5D79; mobile=N",
            "SWID=DAA87BCE-41FF-489E-9D02-6D9D289C42E4; DE2=dXNhO3dhO3JlZG1vbmQ7YnJvYWRiYW5kOzU7NDs0OzgxOTswNDcuNjgzOy0xMjIuMTIzOzg0MDs0ODszNjc7Njt1czs=; gi=usa|wa|redmond|broadband|47.683|-122.123|1386f703; contentLocale=en-US",
            "SWID=DAA87BCE-41FF-489E-9D02-6D9D289C42E4",
            "CB%5FSID=906566811e5f4127a961adf276069d0b-392821251-XQ-6; BID=X13337A65048452C7EEA267382513AE1C62EF3884CF58D7649A25405F7F4A8F7CB603AE92E19E311CC5E656AD0814E4832; JDP=2",
            "MSCulture=IP=131.107.174.143&IPCulture=en-US&PreferredCulture=en-US&PreferredCulturePending=&Country=VVM=&ForcedExpiration=0&timeZone=0&myStuffDma=&myStuffMarket=&USRLOC=QXJlYUNvZGU9NDI1JkNpdHk9UmVkbW9uZCZDb3VudHJ5Q29kZT1VUyZDb3VudHJ5TmFtZT1Vbml0ZWQg",
            "stop_mobi=yes; sid=hPYRfqcoX7iPaOj_8RJL6n4nji2c1a3zCV351Uo5ji2c1SxTXN1dpRV3; pgid=7k1QXIqeQFRSRpwi7eGoyzv80000BNkxDlrn",
            "PREF=ID=29316b0ce99f2112:U=d934d7cddd3d159f:FF=0:TM=1339520430:LM=1339520440:S=rDcP9rKlgT3D5Yx1; NID=60=pA3CflmjMVWTO6ZL1WTTZLA40NKQdSyluLnnl6cs4WdhZIFMjrwekP1M8LLxVU08k85ztslegMvCJRPifMFeYykzP5MX5hElZm5Is5ZSeFAq0z3OKuOD58wnJTkLlZrh",
            "evsessionid=66.235.125.15.1339520453089615; JSESSIONID=Rf6U3wIxUpmQl3IP; abtest=B",
            "tempSessionId=Cg5hyE/XdcyDa66Pang; arrowLat=1339520462696; arrowSpc=2; mad4=a",
            "btc=4; session=ts%3D2; perm=countryCode%3Dus"
        };

        #endregion

        #region Private Fields

        /// <summary>
        /// Random generator for populating exter HTTP headers
        /// </summary>
        static Random RandIndex = new Random((int)DateTime.Now.Ticks);

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SMHeaders"/> class.
        /// </summary>
        public SMHeaders()
        {
            InitExtraHeaders();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SMHeaders"/> class.
        /// </summary>
        /// <param name="info">The serialization info</param>
        /// <param name="context">The serialization context.</param>
        protected SMHeaders(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
            InitExtraHeaders();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Indexer for SMHeaders class
        /// </summary>
        /// <param name="key">New or existing key</param>
        /// <returns>New string</returns>
        public new string this[string key]
        {
            get 
            {
                return ContainsKey(key) ? base[key] : null;
            }

            set
            {
                if (ContainsKey(key))
                {
                    base[key] = value;
                }
                else
                {
                    Add(key, value);
                }
            }
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>returns string representation.</returns>
        public override string ToString()
        {
            string strHeader = string.Empty;

            if (!string.IsNullOrEmpty(this[Path]))
            {
                strHeader += string.Format("path: {0} \n", this[Path]);
            }

            if (!string.IsNullOrEmpty(this[Method]))
            {
                strHeader += string.Format("method: {0} \n", this[Method]);
            }

            if (!string.IsNullOrEmpty(this[Method]))
            {
                strHeader += string.Format("version: {0} \n", this[Version]);
            }

            if (!string.IsNullOrEmpty(this[Host]))
            {
                strHeader += string.Format("host: {0} \n", this[Host]);
            }

            if (!string.IsNullOrEmpty(this[Scheme]))
            {
                strHeader += string.Format("scheme: {0} \n", this[Scheme]);
            }

            if (!string.IsNullOrEmpty(this[Status]))
            {
                strHeader += string.Format("status: {0} \n", this[Status]);
            }

            if (!string.IsNullOrEmpty(this[UserAgent]))
            {
                strHeader += string.Format("user-agent: {0} \n", this[UserAgent]);
            }

            if (!string.IsNullOrEmpty(this[AcceptEncoding]))
            {
                strHeader += string.Format("accept-encoding: {0} \n", this[AcceptEncoding]);
            }

            if (!string.IsNullOrEmpty(this[ContentType]))
            {
                strHeader += string.Format("content-type: {0} \n", this[ContentType]);
            }

            if (string.IsNullOrEmpty(strHeader))
            {
                strHeader = "{Empty}\n";
            }

            return strHeader;
        }

        /// <summary>
        /// Merge two instances of SMHeaders
        /// </summary>
        /// <param name="headers">SMHeaders object</param>
        public void Merge(SMHeaders headers)
        {
            foreach (var smheader in headers)
            {
                this[smheader.Key] = smheader.Value;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Add headers which are not compulsory, but valid. These headers have no impact 
        /// on S+M transfers. We add then to vary compression and encryption load.
        /// </summary>
        private void InitExtraHeaders()
        {
            // add Cache-Control header. From client side we put one attribute
            this[CacheControl] = this.CacheControlValues[RandIndex.Next(0, this.CacheControlValues.Length)];

            // add a Cookies to 50% of all requests
            int toAddCookie = RandIndex.Next(0, 2);
            if (toAddCookie > 0)
            {
                this[Cookie] = this.ClientCookies[RandIndex.Next(0, this.ClientCookies.Length)];
            }
 
        }

        #endregion

    }
}
