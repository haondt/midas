﻿using Newtonsoft.Json;
using SpendLess.Core.Constants;

namespace SpendLess.Domain.NodeRed.Models
{
    public class SendToNodeRedRequestDto
    {
        public string Account { get; set; } = "";
        public string Configuration { get; set; } = "";
        public List<string> Data { get; set; } = [];
        public bool IsFirstRow { get; set; } = false;

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, SpendLessConstants.ApiSerializerSettings);
        }
    }
}