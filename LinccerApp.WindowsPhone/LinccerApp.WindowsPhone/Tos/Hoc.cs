/*
Copyright (C) 2009, 2010, Hoccer GmbH Berlin, Germany <www.hoccer.com>

These coded instructions, statements, and computer programs contain
proprietary information of Linccer GmbH Berlin, and are copy protected
by law. They may be used, modified and redistributed under the terms
of GNU General Public License referenced below.

Alternative licensing without the obligations of the GPL is
available upon request.

GPL v3 Licensing:

This file is part of the "Linccer .Net-API".

Linccer .Net-API is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Linccer .Net-API is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Linccer .Net-API. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using LinccerApi.WindowsPhone;


namespace LinccerApp.WindowsPhone
{
    public class Hoc
    {
        public Hoc ()
        {
            Sender = new Device();
            DataList = new List<HocData>();
        }

        public Device Sender { get; set; }

        [JsonProperty("data")]
        public List<HocData> DataList { get; set; }

        public override string ToString ()
        {
            return JsonConvert.SerializeObject (this, Formatting.None, Utils.DefaultSerializerSettings);
        }

    }

    public class Device
    {
        public Device(){
            ClientId = Guid.NewGuid ().ToString ();
        }

        public Device(string clientId){
            ClientId = clientId;
        }

        [JsonProperty("client-id")]
        public string ClientId { get; set; }

        public string Name { get; set; }

    }

    public class HocData
    {
        public HocData ()
        {
        }

        public string Type { get; set; }

        public string Content { get; set; }

        public string Uri { get; set; }
    }
    
    
}

