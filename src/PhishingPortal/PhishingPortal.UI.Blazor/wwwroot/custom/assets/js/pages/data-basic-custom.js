$(document).ready(function() {
    setTimeout(function() {
        $('#activeCampaign,#allCampaign,#scheduleCampaign,#completedCampaign').DataTable({
            "order": [
                [3, "desc"]
            ]
        });
    }, 350);
});