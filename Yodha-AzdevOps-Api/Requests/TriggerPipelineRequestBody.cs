using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace Yodha.AzDevops.Api.Requests
{
    public class TriggerPipelineRequestBody
    {
        public string UserName { get; set; }
        public bool IsDryRun { get; set; }
        private ActionType _actionType;
        public string ActionType
        {
            get => _actionType.ToString();
            set => Enum.TryParse<ActionType>(value, true, out _actionType);
        }
    }

    public enum ActionType
    {
        [EnumMember( Value = "Deploy" )]
        Deploy,
        [EnumMember( Value = "Publish" )]
        Publish
    }
}
