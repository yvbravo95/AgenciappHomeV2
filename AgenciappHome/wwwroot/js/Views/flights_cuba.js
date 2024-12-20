let searchCount = 0;
$('#origin').select2({ placeholder: "Seleccione un origen" });
$('#destination').select2({ placeholder: "Seleccione un destino" });

document.addEventListener('DOMContentLoaded', (event) => {
    const today = new Date().toISOString().split('T')[0];
    document.getElementById('departure').setAttribute('min', today);
    document.getElementById('return').setAttribute('min', today);

    document.getElementById('departure').addEventListener('change', function () {
        const departureDate = this.value;
        document.getElementById('return').setAttribute('min', departureDate);
    });
});

async function searchFlights() {
    const type = document.getElementById('type').value
    const origin = document.getElementById('origin').value;
    const destination = document.getElementById('destination').value;
    const departureDate = document.getElementById('departure').value;
    const returnDate = document.getElementById('return').value;
    const adults = document.getElementById('adults').value;
    const children = document.getElementById('children').value;
    const infants = document.getElementById('infants').value;

    // validar parametros
    if (!origin || !destination || !departureDate || !returnDate || !adults) {
        toastr.error("Por favor, complete todos los campos!")
        return;
    }

    const flightsTableBody = document.getElementById('flightsTableBody');
    flightsTableBody.innerHTML = '';

    $('#div_loading').show()

    searchCount = 6;

    searchAndDisplay(6, origin, destination, departureDate, returnDate, adults, children, infants);
    searchAndDisplay(0, origin, destination, departureDate, returnDate, adults, children, infants);
    searchAndDisplay(1, origin, destination, departureDate, returnDate, adults, children, infants);
    searchAndDisplay(2, origin, destination, departureDate, returnDate, adults, children, infants);
    searchAndDisplay(3, origin, destination, departureDate, returnDate, adults, children, infants);
    searchAndDisplay(4, origin, destination, departureDate, returnDate, adults, children, infants);
}

async function searchAndDisplay(type, origin, destination, departureDate, returnDate, adults, children, infants) {
    const flights = await search(type, origin, destination, departureDate, returnDate, adults, children, infants);
    searchCount -= 1;
    if (searchCount <= 0) {
        $('#div_loading').hide()
    }

    if (flights.length == 0) {
        if (type == 0) {
            toastr.info("No se obtuvieron vuelos de Cacsuite.")
        }
        else if (type == 1) {
            toastr.info("No se obtuvieron vuelos de HavanaAir.")
        }
        else if (type == 2) {
            toastr.info("No se obtuvieron vuelos de InvictaAir.")
        }
        else if (type == 3) {
            toastr.info("No se obtuvieron vuelos de EasyPax.")
        }
        else if (type == 4) {
            toastr.info("No se obtuvieron vuelos de XaelSuite.")
        }
        else if (type == 6) {
            toastr.info("No se obtuvieron vuelos de GFlight.")
        }
    }

    displayItems(flights)
}

async function search(type, origin, destination, departureDate, returnDate, adults, children, infants) {
    const queryParams = new URLSearchParams({
        type,
        origin,
        destination,
        departureDate,
        returnDate,
        adults,
        children,
        infants
    });

    const url = `/ticket/GetFlightsCuba?${queryParams.toString()}`;

    const res = await fetch(url)
        .then(response => response.json())
        .then(res => res);

    if (!res.success) {
        return [];
    }

    return res.data;

}

function displayItems(flights) {
    if (!flights) return;
    flights.forEach(flight => {
        const row = document.createElement('tr');
        const types = {
            "Cacsuite": "Cubaazul Charter",
            "HavanaAir": "Havana Air",
            "InvictaAir": "Invicta Air",
            "EasyPax": "Aereocuba",
            "XaelSuite": "Xael Charter",
            "AmericanAirline": "American Airline"
        }
        flight.type = types[flight.type] ?? flight.type;

        row.innerHTML = `
                                    <td>${flight.type}</td>
                                    <td>${flight.departFlight?.flightNumber}</td>
                                    <td>${flight.returnFlight?.flightNumber}</td>
                                    <td>${flight.departFlight?.departureTime}</td>
                                    <td>${flight.returnFlight?.departureTime}</td>
                                    <td>${flight.price}</td>
                                `;
        flightsTableBody.appendChild(row);
    });
}