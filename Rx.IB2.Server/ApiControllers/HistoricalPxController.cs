using Microsoft.AspNetCore.Mvc;
using Rx.IB2.Models;
using Rx.IB2.Services;

namespace Rx.IB2.ApiControllers;

[Route("api/[controller]")]
[ApiController]
public class HistoricalPxController : ControllerBase {
    private IbApiSender Sender { get; }

    private IbApiHistoryPxRequestManager HistoryPxRequestManager { get; }

    public HistoricalPxController(IbApiSender sender, IbApiHistoryPxRequestManager historyPxRequestManager) {
        Sender = sender;
        HistoryPxRequestManager = historyPxRequestManager;
    }

    [HttpGet]
    public ActionResult<IEnumerable<PxDataBarModel>> GetHistoricalData() {
        try {
            // ---- Reserved for future
            // var symbol = Request.Query.GetFirstValueInQuery("symbol");
            // var barSize = Request.Query.GetFirstValueInQuery("barSize").ToBarSize();
            // var securityType = Request.Query.GetFirstValueInQuery("securityType").ToSecurityType();
            // var durationUnit = Request.Query.GetFirstValueInQuery("durationUnit").ToDurationUnit();
            // var durationValue = Convert.ToInt32(Request.Query.GetFirstValueInQuery("durationValue"));
            // var rthOnly = Convert.ToBoolean(Request.Query.GetFirstValueInQuery("rthOnly"));

            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        } catch (ArgumentException ex) {
            return BadRequest(ex.Message);
        }
    }
}