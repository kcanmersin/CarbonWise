using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Application.Services.LLMService
{
    public interface ILlmService
    {
        Task<LlmResponse> GenerateContentAsync(LlmRequest request);
    }
}
