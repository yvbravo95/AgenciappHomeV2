
async function GetGuideByNumber(number) {
    var guide = await $.get("/OrderCubiq/GetGuiaAereaByNumber?number=" + number);
    return guide;
}