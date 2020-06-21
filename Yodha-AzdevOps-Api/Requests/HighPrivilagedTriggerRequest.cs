namespace Yodha.AzDevops.Api.Requests
{
    public class HighPrivilageTrigger<T>
    {
        public TriggerContext TriggerContext { get; set; }
        public T TriggerData { get; set; }
        public string TriggerReason { get; set; }
        public bool IsValidation { get; set; }
    }
    public class TriggerContext
    {

    }
}