// Populate form fields with query parameters if available
const urlParams = new URLSearchParams(window.location.search);
const startDate = urlParams.get('startDate');
const endDate = urlParams.get('endDate');

if (startDate) document.getElementById('startDate').value = startDate;
if (endDate) document.getElementById('endDate').value = endDate;

// Handle form submission
$('#btn_filtrar').on('click', function () {
    event.preventDefault(); // Prevent default form submission
    const startDate = document.getElementById('startDate').value;
    const endDate = document.getElementById('endDate').value;

    const queryParams = new URLSearchParams();
    if (startDate) queryParams.append('startDate', startDate);
    if (endDate) queryParams.append('endDate', endDate);

    window.location.search = queryParams.toString();
});