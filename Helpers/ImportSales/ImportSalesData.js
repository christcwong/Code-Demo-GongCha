function CallDateTime() {
    GongChaWebApplication.Helpers.ImportSales.ImportSalesDataWebService.OurServerOutput(OnSucceeded);
}

function OnSucceeded(result) {
    var lblOutput = document.getElementById("lblOutput");
    lblOutput.innerHTML = result;
}