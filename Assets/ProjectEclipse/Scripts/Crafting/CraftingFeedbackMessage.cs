using System.Collections.Generic;

namespace ProjectEclipse.Crafting
{
    public class CraftingFeedbackMessage
    {
        public string Header { get; private set; }
        public string Detail { get; private set; }
        public bool IsError { get; private set; }
        public bool IsSuccess { get; private set; }
        public List<CraftingRequirementLine> Lines { get; private set; }

        public CraftingFeedbackMessage(
            string header,
            string detail,
            bool isError,
            bool isSuccess,
            IEnumerable<CraftingRequirementLine> lines)
        {
            Header = header;
            Detail = detail;
            IsError = isError;
            IsSuccess = isSuccess;
            Lines = lines != null ? new List<CraftingRequirementLine>(lines) : new List<CraftingRequirementLine>();
        }
    }
}
