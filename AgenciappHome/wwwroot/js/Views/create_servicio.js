let saving = false;

$(document).ready(function () {
    var productos;
    var precioProductos = 0;
  //id del cliente para que se cree uno nuevo
  var cServicio = 0;
  var cost = 0;
  var price = 0;
  var wholesaleCost = 0;
  var wholesalePrice = 0;
  var isPackage = false;
    var data = [{
        name: "",
        lastName: "",
        adulto: true
    }];

  var parseDate = function (value) {
    if (value == "0001-01-01") {
      return "";
    }
    var m = value.match(/^(\d{4})(\/|-)?(\d{1,2})(\/|-)?(\d{1,2})$/);
    if (m)
      value =
        ("00" + m[3]).slice(-2) + "/" + ("00" + m[5]).slice(-2) + "/" + m[1];

    return value;
  };

  var loadCiudad = function (estado, ciudad) {
    if (!estado) return;
    $.ajax({
      url: "/Provincias/Ciudades?nombre=" + estado.val(),
      type: "POST",
      dataType: "json",
      success: function (response) {
        ciudad.empty();
        ciudad.append(new Option());
        for (var i in response) {
          var m = response[i];
          ciudad.append(new Option(m, m));
          if (
            $(ciudad).data("value") != "" &&
            $(ciudad).data("value") != null
          ) {
            $(ciudad).val($(ciudad).data("value")).trigger("change");
          }
        }
      },
    });
  };

  $(".step-icon").each(function () {
    var $this = $(this);
    if ($this.siblings("span.step").length > 0) {
      $this.siblings("span.step").empty();
      $(this).appendTo($(this).siblings("span.step"));
    }
  });

  var i = 0;
  if (paqueteId != "00000000-0000-0000-0000-000000000000") {
    i = 1;
  }
  $("#zc").steps({
    headerTag: "h6",
    bodyTag: "fieldset",
    transitionEffect: "fade",
    titleTemplate: '<span class="step">#index#</span> #title#',
    enableCancelButton: true,
    startIndex: i,
    labels: {
      previous: "Anterior",
      next: "Siguiente",
      finish: "Crear",
      cancel: "Cancelar",
    },
      onCanceled: function () {
          if (paqueteId != "00000000-0000-0000-0000-000000000000") {
              window.location = `/PaqueteTuristico/Create?IdPaquete=${paqueteId}`;
          }
          else {
              window.location = "/Servicios";
          }
    },
    onStepChanging: function (event, currentIndex, newIndex) {
      // Allways allow previous action even if the current form is not valid!
      if (currentIndex > newIndex) {
        return true;
      }

      //----------------Step1
      var error = false;

      if (newIndex == 0) {
        alert("df   ");
      }

      if (newIndex == 1) {
        if ($("#selectClient").val() == "") {
          showWarningMessage("Atención", "El campo cliente es obligatorio.");
          return false;
        }
      }

      //----------------Step2

      return true;
    },
      onFinishing: function (event, currentIndex) {
          event.preventDefault();
          if (!saving) {
              saving = true;
              $("#Data").val(JSON.stringify(data));
              //Verificar Mayorista
              if (
                  $("#selectMayorista").val() == null ||
                  $("#selectMayorista").val() == ""
              ) {
                  var verify = confirm("¿Desea crear el trámite sin mayorista?");
                  if (verify) {
                      $("#zc").submit();
                  }
              } else {
                  $("#zc").submit();
              }
          }
    },
    onFinished: function (event, currentIndex) {
      event.preventDefault();
    },
  });
  $("[href='#cancel']").addClass("btn-danger");

    $("#selectProduct").select2({
        placeholder: "Buscar producto en la bodega",
        width: "100%"
    });

  $("#selectMayorista").select2({
    placeholder: "Seleccione un mayorista",
    val: null,
    width: "100%",
  });
  $("#tipoServicio").select2({
    placeholder: "Seleccione un servicio",
    val: null,
    width: "100%",
  });
  $("#subServicio").select2({
    placeholder: "Seleccione un subservicio",
    val: null,
    width: "100%",
  });
  $("#tipoPago").select2({
    placeholder: "Tipo Pago",
    val: null,
    width: "100%",
  });
  $(".Sel").select2({
    placeholder: "Buscar cliente por teléfono, nombre o apellido",
    val: null,
    width: "100%",
    ajax: {
      type: "POST",
      url: "/Clients/findClient",
      data: function (params) {
        var query = {
          search: params.term,
        };

        // Query parameters will be ?search=[term]&type=public
        return query;
      },
      processResults: function (data) {
        // Transforms the top-level key of the response object from 'items' to 'results'
        return {
          results: $.map(data, function (obj) {
            return { id: obj.clientId, text: obj.fullData };
          }),
        };
      },
    },
  });

  // Cuando se cree un nuevo cliente al recargarse se muestre
  if (idCliente != "00000000-0000-0000-0000-000000000000") {
    $.ajax({
      type: "POST",
      url: "/Clients/GetClient",
      dataType: "json",
      contentType: "application/json; charset=utf-8",
      data: JSON.stringify(idCliente),
      async: false,
      success: function (data) {
        var newOption = new Option(
          data["movile"] + "-" + data["name"] + " " + data["lastname"],
          idCliente,
          false,
          false
        );
        $("#cliente").append(newOption);
        $("#cliente")
          .val(idCliente)
          .trigger("change")
          .trigger("select2:select");
        $("#editarCliente").removeClass("hidden");

        //Datos del Cliente en Authorization Card
        $("#nameClientCard").html(
          "<strong>Nombre: </strong>" + data.name + " " + data.lastName
        );
        $("#phoneClientCard").html("<strong>Teléfono: </strong>" + data.movil);
        $("#emailClientCard").html("<strong>Email: </strong>" + data.email);
        $("#countryClientCard").html("<strong>País: </strong>" + data.country);
        $("#cityClientCard").html("<strong>Ciudad: </strong>" + data.city);
        $("#addressClientCard").html(
          "<strong>Dirección: </strong>" + data.calle
        );
        $("#AuthaddressOfSend").val(data.calle);
        $("#Authemail").val(data.email);
        $("#Authphone").val(data.movil);

        //Datos del Cliente en Step 1
        $("#inputClientName").val(data.name);
        $("#inputClientLastName").val(data.lastName);
        $("#inputClientName2").val(data.name2);
        $("#inputClientLastName2").val(data.lastName2);
        $("#inputClientMovil").val(data.movil);
        $("#inputClientEmail").val(data.email);
        $("#inputClientAddress").val(data.calle);
        $("#inputClientCity").data("value", data.city);
        $("#inputClientZip").val(data.zip);
        $("#inputClientState").val(data.state).trigger("change");
        loadCiudad($("#inputClientState"), $("#inputClientCity"));
        $("#inputClientFNaci").val(parseDate(data.fechaNac.slice(0, 10)));
        $("#inputClientID").val(data.id);

        if (data.getCredito && data.getCredito != 0) {
          $("#div_credito").removeAttr("hidden");
          $("#credito").html(data.getCredito);
        } else {
          $("#div_credito").attr("hidden", "hidden");
        }
        if (data.conflictivo) {
          $("#conflictivo").removeAttr("hidden");
        } else {
          $("#conflictivo").attr("hidden", "hidden");
        }
      },
      failure: function (response) {
        showErrorMessage("ERROR", response.statusText);
      },
      error: function (response) {
        showErrorMessage("ERROR", response.statusText);
      },
    });
  }

  $("#selectMayorista").on("change", function () {
    var id = $(this).val();
    $.ajax({
      type: "POST",
      url: "/Servicios/getMayorista",
      data: {
        id: id,
      },
      async: false,
      success: function (data) {
        wholesaleCost = data.costoMayorista;
        wholesalePrice = data.precioVenta;
        if (wholesaleCost == 0) $('[name = "costo"]').val(cost);
        else $('[name = "costo"]').val(wholesaleCost);

        if (wholesalePrice == 0) $('[name = "importe"]').val(price);
        else $('[name = "importe"]').val(wholesalePrice);

        calculate();
      },
      failure: function (response) {
        showErrorMessage("ERROR", response.responseText);
      },
      error: function (response) {
        showErrorMessage("ERROR", response.responseText);
      },
    });
  });

  function getMayoristas() {
    //Pongo los valores en 0
    $('[name = "costo"]').val(0);
    $('[name = "importeTotal"]').val(0);
    $('[name = "importePagado"]').val(0);
    $('[name = "utilidad"]').val(0);
    $('[name = "cantidadOrdenes_Tienda"]').val(0);

    var Idservicio = $("#tipoServicio").val();
    $.ajax({
      type: "POST",
      url: "/Servicios/getMayoristas",
      data: {
        Idservicio: Idservicio,
      },
      async: false,
      success: function (data) {
        cServicio = data.costo;
        cost = data.wholesaleCost;
        price = data.price;

        $("#selectMayorista").empty();
        $("#selectMayorista").append(
          new Option("Seleccione un mayorista", "", true)
        );

        if (data.mayoristas.length != 0) {
          for (var i = 0; i < data.mayoristas.length; i++) {
            $("#selectMayorista").append(
              new Option(data.mayoristas[i].name, data.mayoristas[i].id)
            );
          }
        } else {
          wholesaleCost = 0;
          wholesalePrice = 0;
        }
        $("#CostoXServicio").val(cServicio);
          $('[name = "costo"]').val(cost);
          $('[name = "importe"]').val(price + precioProductos);

        //$('[name = "importeTotal"]').val(0);
        calculate();
      },
      failure: function (response) {
        showErrorMessage("ERROR", response.responseText);
      },
      error: function (response) {
        showErrorMessage("ERROR", response.responseText);
      },
    });
  }

  function getMayoristasSub() {
    //Pongo los valores en 0
    $('[name = "costo"]').val(0);
    $('[name = "importeTotal"]').val(0);
    $('[name = "importePagado"]').val(0);
    $('[name = "utilidad"]').val(0);
    $('[name = "cantidadOrdenes_Tienda"]').val(0);

    var Idservicio = $("#subServicio").val();
    $.ajax({
      type: "POST",
      url: "/Servicios/getMayoristas",
      data: {
        Idservicio: Idservicio,
      },
      async: false,
      success: function (data) {
        cServicio = data.costo;
        cost = data.wholesaleCost;
        price = data.price;
        $("#selectMayorista").empty();
        $("#selectMayorista").append(
          new Option("Seleccione un mayorista", "", true)
        );

        if (data.mayoristas.length != 0) {
          for (var i = 0; i < data.mayoristas.length; i++) {
            $("#selectMayorista").append(
              new Option(data.mayoristas[i].name, data.mayoristas[i].id)
            );
          }
        } else {
          wholesaleCost = 0;
          wholesalePrice = 0;
        }

        $("#CostoXServicio").val(cServicio);
          $('[name = "costo"]').val(cost);
          $('[name = "importe"]').val(price + precioProductos);

        calculate();
      },
      failure: function (response) {
        showErrorMessage("ERROR", response.responseText);
      },
      error: function (response) {
        showErrorMessage("ERROR", response.responseText);
      },
    });
  }

  function getSubServicio() {
    var Idservicio = $("#tipoServicio").val();
    $.ajax({
      type: "POST",
      url: "/Servicios/getSubServicios",
      data: {
        Idservicio: Idservicio,
      },
      async: false,
      success: function (data) {
        $("#subServicio").empty();
        $("#subServicio").append(
          new Option("Seleccione un subservicio", "", true)
        );

        if (data.length != 0) {
          for (var i = 0; i < data.length; i++) {
            $("#subServicio").append(new Option(data[i].name, data[i].id));
          }
          getMayoristas();
          $("#subServicioDiv").show();
        } else {
          $("#subServicioDiv").hide();
          getMayoristas();
        }
      },
      failure: function (response) {
        showErrorMessage("ERROR", response.responseText);
      },
      error: function (response) {
        showErrorMessage("ERROR", response.responseText);
      },
    });
    }

    function updatePackage() {
        if (isPackage) {
            $("#package_div").removeClass("hidden");
        }
        else {
            $("#data").val("");
            $("#package_div").addClass("hidden");
        }
    }

    $(".packagename").on("change", (e) => {
        let index = $(e.target).data("index");
        data[index].name = $(e.target).val();
    })

    $(".packagelastname").on("change", (e) => {
        let index = $(e.target).data("index");
        data[index].lastName = $(e.target).val();
    })

    $(".packgecheckbox").on("change", (e) => {
        let index = $(e.target).data("index");
        data[index].adulto = $(e.target).is(":checked");
        console.log(data)
    })

    $("#package-count").on("change", (e) => {
        let value = $(e.target).val();
        if (value > data.length) {
            let count = value - data.length;
            for (var i = 0; i < count; i++) {
                $("#package_div").append(`\
                    <hr id="hr${data.length}"/>\
                    <div class="row" id="div${data.length}">\
                        <div class="col-md-5">\
                            <input type="text" class="form-control" id="packagename${data.length}" data-index="${data.length}" required />\
                        </div>\
                        <div class="col-md-5">\
                            <input type="text" class="form-control" id="packagelastname${data.length}" data-index="${data.length}" required />\
                        </div>\
                        <div class="col-md-2">\
                            <label>¿Adulto? <input type="checkbox" checked class="custom-checkbox" id="packgecheckbox${data.length}" data-index="${data.length}" /></label>\
                        </div>\
                    </div>\
                `);
                $(`#packagename${data.length}`).on("change", (e) => {
                    let index = $(e.target).data("index");
                    data[index].name = $(e.target).val();
                })

                $(`#packagelastname${data.length}`).on("change", (e) => {
                    let index = $(e.target).data("index");
                    data[index].lastName = $(e.target).val();
                })

                $(`#packgecheckbox${data.length}`).on("change", (e) => {
                    let index = $(e.target).data("index");
                    data[index].adulto = $(e.target).is(":checked");
                    console.log(data)
                })
                data.push({ name: "", lastName: "", adulto: true });
            }
        }
        else {
            let del = data.length - value;
            console.log(del);
            for (var i = 0; i < del; i++) {
                data.pop();
                $(`#hr${data.length}`).remove();
                $(`#div${data.length}`).remove();
            }
        }
    })

  // La seleccionarse un servicio se cargan los mayoristas de ese servicio
    $("#tipoServicio").on("change", function () {
        isPackage = $('option[value="' + $(this).val() + '"]').data("ispackage") == "True";
        updatePackage();
        var servicio = $('option[value="' + $(this).val() + '"]').html();
        getSubServicio();
        if (servicio == "Tienda") {
          $('[name="servTienda"]').show();
        }
        else {
          $('[name="servTienda"]').hide();
        }

        if (servicio == "Poder Menor") {
            $('#container-minorauth').show();
        }
        else {
            $('#container-minorauth').hide();
        }
  });

  $("#subServicio").on("change", function () {
    getMayoristasSub();
  });

  //Cliente
  function showClient() {
    selectedClient = $("#cliente").val();
    $("#editarCliente").removeClass("hidden");

    $.ajax({
      type: "POST",
      url: "/Clients/GetClient",
      dataType: "json",
      contentType: "application/json; charset=utf-8",
      data: JSON.stringify(selectedClient),
      async: false,
      success: function (data) {
        //Datos del Cliente en Authorization Card
        $("#nameClientCard").html(
          "<strong>Nombre: </strong>" + data.name + " " + data.lastName
        );
        $("#phoneClientCard").html("<strong>Teléfono: </strong>" + data.movil);
        $("#emailClientCard").html("<strong>Email: </strong>" + data.email);
        $("#countryClientCard").html("<strong>País: </strong>" + data.country);
        $("#cityClientCard").html("<strong>Ciudad: </strong>" + data.city);
        $("#addressClientCard").html(
          "<strong>Dirección: </strong>" + data.calle
        );
        $("#AuthaddressOfSend").val(data.calle);
        $("#Authemail").val(data.email);
        $("#Authphone").val(data.movil);

        $("#inputClientName").val(data.name);
        $("#inputClientLastName").val(data.lastName);
        $("#inputClientName2").val(data.name2);
        $("#inputClientLastName2").val(data.lastName2);
        $("#inputClientMovil").val(data.movil);
        $("#inputClientEmail").val(data.email);
        $("#inputClientAddress").val(data.calle);
        $("#inputClientState").val(data.state).trigger("change");
        $("#inputClientCity").data("value", data.city);
        loadCiudad($("#inputClientState"), $("#inputClientCity"));
        $("#inputClientZip").val(data.zip);
        $("#inputClientFNaci").val(parseDate(data.fechaNac.slice(0, 10)));
        $("#inputClientID").val(data.id);

        if (data.getCredito && data.getCredito != 0) {
          $("#div_credito").removeAttr("hidden");
          $("#credito").html(data.getCredito);
        } else {
          $("#div_credito").attr("hidden", "hidden");
        }
        if (data.conflictivo) {
          $("#conflictivo").removeAttr("hidden");
        } else {
          $("#conflictivo").attr("hidden", "hidden");
        }
      },
      failure: function (response) {
        showErrorMessage("ERROR", response.statusText);
      },
      error: function (response) {
        showErrorMessage("ERROR", response.statusText);
      },
    });
  }

  $(".date")
    .datetimepicker({
      format: "MM/DD/YYYY",
      viewMode: "years",
      widgetPositioning: {
        horizontal: "auto",
        vertical: "bottom",
      },
      extraFormats: ["YYYY-MM-DD"],
    })
    .on("dp.change", function (e) {
      var mirrorId = $(this).data("input");

      if (mirrorId) {
        var mirror = $("#" + mirrorId);
        mirror
          .val(new Date(e.date).toISOString().substr(0, 10))
          .trigger("change");
      }
      var d = new Date(e.date);
      if (d < new Date("1920-01-01") || d > new Date("2030-12-31")) {
        showWarningMessage("la fecha esta en un rango inválido");
      }
    });

  var validateEditarCliente = function () {
    if ($("#inputClientName").val() == "") {
      showWarningMessage("Atención", "El campo Nombre no puede estar vacío.");
      return false;
    } else if ($("#inputClientLastName").val() == "") {
      showWarningMessage(
        "Atención",
        "El campo Apellidos no puede estar vacío."
      );
      return false;
    } else if ($("#inputClientMovil").val() == "") {
      showWarningMessage("Atención", "El campo Teléfono no puede estar vacío.");
      return false;
    }

    if ($("#inputClientEmail").val() != "") {
      var regexEmail = /^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$/;
      if (!regexEmail.test($("#inputClientEmail").val())) {
        showWarningMessage(
          "Atención",
          "El campo Email no tiene el formato correcto."
        );
        return false;
      }
    }

    return true;
  };
  var desactClientForm = function () {
    $("#nuevoCliente").removeAttr("disabled");
    $(".select2-placeholder-selectClient").removeAttr("disabled");

    $("#inputClientName").attr("disabled", "disabled");
    $("#inputClientLastName").attr("disabled", "disabled");
    $("#inputClientName2").attr("disabled", "disabled");
    $("#inputClientLastName2").attr("disabled", "disabled");
    $("#inputClientMovil").attr("disabled", "disabled");
    $("#inputClientEmail").attr("disabled", "disabled");
    $("#inputClientAddress").attr("disabled", "disabled");
    $("#inputClientCity").attr("disabled", "disabled");
    $("#inputClientState").attr("disabled", "disabled");
    $("#inputClientZip").attr("disabled", "disabled");
    $("#inputClientFNaci").attr("disabled", "disabled");
    $("#inputClientID").attr("disabled", "disabled");

    $("#editarCliente").removeClass("hidden");
    $("#cancelarCliente").addClass("hidden");
    $("#guardarCliente").addClass("hidden");

    $("a[href='#next']").removeClass("hidden");
  };
  var cancelClientForm = function () {
    $("#inputClientName").val($("#inputClientName").data("prevVal"));
    $("#inputClientLastName").val($("#inputClientLastName").data("prevVal"));
    $("#inputClientName2").val($("#inputClientName2").data("prevVal"));
    $("#inputClientLastName2").val($("#inputClientLastName2").data("prevVal"));
    $("#inputClientMovil").val($("#inputClientMovil").data("prevVal"));
    $("#inputClientEmail").val($("#inputClientEmail").data("prevVal"));
    $("#inputClientAddress").val($("#inputClientAddress").data("prevVal"));
    $("#inputClientCity").val($("#inputClientCity").data("prevVal"));
    $("#inputClientState")
      .val($("#inputClientState").data("prevVal"))
      .trigger("change");
    $("#inputClientZip").val($("#inputClientZip").data("prevVal"));
    $("#inputClientID").val($("#inputClientID").data("prevVal"));
    $("#inputClientFNaci").val($("#inputClientFNaci").data("prevVal"));

    desactClientForm();
  };

  $("#cliente").on("select2:select", function (a, b) {
    $("#editarCliente").removeClass("hidden");
    showClient();
  });

  $("#editarCliente").on("click", function () {
    // para que no pueda crear nuevo cliente mientras edita cliente
    $("#nuevoCliente").attr("disabled", "disabled");

    // para que no pueda cambiar de cliente mientras edita cliente
    $("#cliente").attr("disabled", "disabled");

    $("a[href='#next']").addClass("hidden");

    $("#inputClientName")
      .removeAttr("disabled")
      .data("prevVal", $("#inputClientName").val());
    $("#inputClientLastName")
      .removeAttr("disabled")
      .data("prevVal", $("#inputClientLastName").val());
    $("#inputClientName2")
      .removeAttr("disabled")
      .data("prevVal", $("#inputClientName2").val());
    $("#inputClientLastName2")
      .removeAttr("disabled")
      .data("prevVal", $("#inputClientLastName2").val());
    $("#inputClientMovil")
      .removeAttr("disabled")
      .data("prevVal", $("#inputClientMovil").val());
    $("#inputClientEmail")
      .removeAttr("disabled")
      .data("prevVal", $("#inputClientEmail").val());
    $("#inputClientAddress")
      .removeAttr("disabled")
      .data("prevVal", $("#inputClientAddress").val());
    $("#inputClientCity")
      .removeAttr("disabled")
      .data("prevVal", $("inputClientCity").val());
    $("#inputClientState")
      .removeAttr("disabled")
      .data("prevVal", $("#inputClientState").val());
    $("#inputClientZip")
      .removeAttr("disabled")
      .data("prevVal", $("#inputClientZip").val());
    $("#inputClientFNaci")
      .removeAttr("disabled")
      .data("prevVal", $("#inputClientFNaci").val());
    $("#inputClientID")
      .removeAttr("disabled")
      .data("prevVal", $("#inputClientID").val());

    $("#editarCliente").addClass("hidden");
    $("#cancelarCliente").removeClass("hidden");
    $("#guardarCliente").removeClass("hidden");
  });
  $("#cancelarCliente").click(cancelClientForm);
  $("#guardarCliente").on("click", function () {
    if (validateEditarCliente()) {
      $.ajax({
        async: true,
        type: "POST",
        contentType: "application/x-www-form-urlencoded",
        data: {
          clientId: $("#cliente").val(),
          name: $("#inputClientName").val(),
          name2: $("#inputClientName2").val(),
          lastname: $("#inputClientLastName").val(),
          lastname2: $("#inputClientLastName2").val(),
          email: $("#inputClientEmail").val(),
          phone: $("#inputClientMovil").val(),
          address: $("#inputClientAddress").val(),
          city: $("#inputClientCity").val(),
          state: $("#inputClientState").val(),
          zip: $("#inputClientZip").val(),
          fechaNac: $("#inputClientFNaci").val(),
          id: $("#inputClientID").val(),
        },
        url: "/Clients/EditClientNew",
        success: function (response) {
          if (response.success) {
            toastr.success(response.msg);
          } else {
            toastr.error(response.msg);
          }
          showClient();
        },
        failure: function (response) {
          showErrorMessage("ERROR", response.responseText);
        },
        error: function (response) {
          showErrorMessage("ERROR", response.responseText);
        },
      });
      desactClientForm();
    }
  });

  //Pagos
  $("#checkpago").on("click", function () {
    if ($("#checkpago").is(" :checked")) {
      $("#untipopago").attr("hidden", "hidden");
      $(".multipopago").removeAttr("hidden");
      $("#contfee").hide();
      $("#ValorPagado").val(0);
    } else {
      $(".multipopago").attr("hidden", "hidden");
      $("#untipopago").removeAttr("hidden");
      $("#pagoCash").val(0);
      $("#pagoZelle").val(0);
      $("#pagoCheque").val(0);
      $("#pagoCredito").val(0);
      $("#pagoTransf").val(0);
      $("#pagoWeb").val(0);
      $("#pagoFinance").val(0);
    }
    calculate();
  });
  $("#tipoPago").on("change", function () {
    var Id = $(this).val();
    tipoPago = $('option[value = "' + Id + '"]').html();
    if (tipoPago == "Crédito o Débito") {
      $("#contfee").show();
      $("#AddAuthorization").show();
      calculate();
    } else {
      $("#contfee").hide();
      $("#AddAuthorization").hide();
      calculate();
    }
    if (
      tipoPago == "Zelle" ||
      tipoPago == "Cheque" ||
      tipoPago == "Transferencia Bancaria"
    ) {
      $("#contNotaPago").show();
    } else {
      $("#contNotaPago").hide();
    }
  });
  $("#check_credito").on("click", function () {
    $("#pagoCash").val(0);
    $("#pagoZelle").val(0);
    $("#pagoCheque").val(0);
    $("#pagoCredito").val(0);
    $("#pagoTransf").val(0);
    $("#pagoFinance").val(0);
    $("#pagoWeb").val(0);
    if ($("#check_credito").is(" :checked")) {
      $("#inputCredito").val($("#credito").html());
    } else {
      $("#inputCredito").val(0);
    }
    calculate();
  });
  $(
    "#cantidadOrdenes_Tienda,#importe,#pagoCash,#pagoZelle,#pagoCheque,#pagoCredito,#pagoTransf,#pagoWeb,#pagoFinance"
  ).on("change", validarInputVacios);
  $(
    "#cantidadOrdenes_Tienda,#importe,#pagoCash,#pagoZelle,#pagoCheque,#pagoCredito,#pagoTransf,#pagoWeb,#pagoFinance"
  )
    .on("keyup", calculate)
    .on("change", calculate);
  $("#ValorPagado")
    .on("keyup", calculatePayment)
    .on("change", calculatePayment);

  function calculate() {
    if (paqueteId != "00000000-0000-0000-0000-000000000000") {
      calculatePayment();
    } else {
      var importeTotal = parseFloat($("#importe").val());
      var costo = parseFloat($("#CostoXServicio").val());
      importeTotal += costo;
      var servicio = $(
        'option[value="' + $("#tipoServicio").val() + '"]'
      ).html();
      if (servicio == "Tienda") {
        var cantOrdenes = parseFloat(
          $('[name="cantidadOrdenes_Tienda"]').val()
          );
          const fee = 20 * cantOrdenes;
          const discountFee = importeTotal - fee
          const costo = discountFee - (discountFee * 0.05)

          $('[name="costo"]').val(costo.toFixed(2));
          $('[name="utilidad"]').val((importeTotal - costo).toFixed(2));
      }

      if ($("#checkpago").is(":checked")) {
        var pagoCash = parseFloat($("#pagoCash").val());
        var pagoZelle = parseFloat($("#pagoZelle").val());
        var pagoCheque = parseFloat($("#pagoCheque").val());
        var pagoTransf = parseFloat($("#pagoTransf").val());
        var pagoWeb = parseFloat($("#pagoWeb").val());
          var pagoCrDeb = parseFloat($("#pagoCredito").val());
        var pagoFinance = parseFloat($("#pagoFinance").val());

        var pagoCrDeb = parseFloat($("#pagoCredito").val());
        var pagoCrDebReal = parseFloat(
          parseFloat($("#pagoCredito").val()) /
            (1 + parseFloat($("#fee").html()) / 100)
        );
        var feeCrDeb = pagoCrDeb - pagoCrDebReal;
        if (pagoCrDeb > 0) {
          $("#contfee").show();
        } else {
          $("#contfee").hide();
        }
        importeTotal += feeCrDeb;

        var pagoCredito = 0;
        if ($("#check_credito").is(":checked")) {
          pagoCredito = parseFloat($("#credito").html()).toFixed(2);
          pagoCredito =
            pagoCredito > precioTotalValue ? importeTotal : pagoCredito;
        }

        balanceValue =
          importeTotal -
          pagoCash -
          pagoZelle -
          pagoCheque -
          pagoCrDeb -
          pagoTransf -
          pagoWeb -
          pagoFinance -
          pagoCredito;

        $("#pagoCash").attr("max", (balanceValue + pagoCash).toFixed(2));
        $("#pagoZelle").attr("max", (balanceValue + pagoZelle).toFixed(2));
        $("#pagoCheque").attr("max", (balanceValue + pagoCheque).toFixed(2));
        $("#pagoTransf").attr("max", (balanceValue + pagoTransf).toFixed(2));
        $("#pagoFinance").attr("max", (balanceValue + pagoFinance).toFixed(2));
        $("#pagoWeb").attr("max", (balanceValue + pagoWeb).toFixed(2));
        $("#pagoCredito").attr(
          "max",
          (
            balanceValue +
            pagoCrDebReal +
            ((balanceValue + pagoCrDebReal) * parseFloat($("#fee").html())) /
              100
          ).toFixed(2)
        );
        $("#pagar_credit").html(
          "$" +
            (
              balanceValue +
              pagoCrDebReal +
              ((balanceValue + pagoCrDebReal) * parseFloat($("#fee").html())) /
                100
            ).toFixed(2) +
            " (" +
            (
              ((balanceValue + pagoCrDebReal) * parseFloat($("#fee").html())) /
              100
            ).toFixed(2) +
            " fee)"
        );

        //Valor Sale Amount en authorization card
        $("#AuthSaleAmount").val(pagoCrDebReal.toFixed(2));
        var aconvcharge = parseFloat($("#AuthConvCharge").val());
        var total = pagoCrDebReal + (pagoCrDebReal * aconvcharge) / 100;
        $("#TotalCharge").val(total.toFixed(2));
      } else {
        tipoPagoId = $("#tipoPago").val();
        tipoPago = $('option[value = "' + tipoPagoId + '"]').html();

        if (tipoPago == "Crédito o Débito") {
          var fee = parseFloat($("#fee").html());
          //Valor Sale Amount en authorization card
          $("#AuthSaleAmount").val(importeTotal.toFixed(2));
          importeTotal = importeTotal + importeTotal * (fee / 100);
          $("#TotalCharge").val(importeTotal.toFixed(2));
        }
        var balanceValue = 0;
        if ($("#check_credito").is(":checked")) {
          if (importeTotal.toFixed(2) - parseFloat($("#credito").html()) > 0) {
            $("#ValorPagado").attr(
              "max",
              importeTotal.toFixed(2) - parseFloat($("#credito").html())
            );
            $("#ValorPagado").val(
              (
                importeTotal.toFixed(2) - parseFloat($("#credito").html())
              ).toFixed(2)
            );
          } else {
            $("#ValorPagado").attr("max", 0);
            $("#ValorPagado").val(0);
          }
        } else {
          $("#ValorPagado").val(importeTotal.toFixed(2));
          $("#ValorPagado").attr("max", importeTotal.toFixed(2));
        }
      }

      $("#debe").val(balanceValue.toFixed(2));
      if ($("#debe").val() == "-0.00") $("#debe").val("0.00");

      $("#importeTotal").val(importeTotal.toFixed(2));
    }
  }
  function calculatePayment() {
    var pagado = $("#ValorPagado").val();
    var precioTotalValue = parseFloat($("#importe").val());
    var max = precioTotalValue;

    if (tipoPago == "Crédito o Débito") {
      var fee = parseFloat($("#fee").html());
      var pagdoReal = pagado / (1 + fee / 100);
      var feeCrDeb = pagado - pagdoReal;
      precioTotalValue = precioTotalValue + feeCrDeb;
      max = max + (max * fee) / 100;

      //Valor Sale Amount en authorization card
      $("#AuthSaleAmount").val(pagdoReal.toFixed(2));
      $("#TotalCharge").val(pagado);
    }
    precioTotalValue = precioTotalValue.toFixed(2);
    var balanceValue = 0;
    balanceValue = precioTotalValue - pagado;

    $("#importeTotal").val(precioTotalValue);
    $("#debe").val(balanceValue.toFixed(2));
    $("#ValorPagado").attr("max", max.toFixed(2));
  }
  function validarInputVacios() {
    if ($("#pagoCash").val() == "") $("#pagoCash").val(0);
    if ($("#pagoZelle").val() == "") $("#pagoZelle").val(0);
    if ($("#pagoCheque").val() == "") $("#pagoCheque").val(0);
    if ($("#pagoCredito").val() == "") $("#pagoCredito").val(0);
    if ($("#pagoTransf").val() == "") $("#pagoTransf").val(0);
    if ($("#pagoFinance").val() == "") $("#pagoFinance").val(0);
    if ($("#pagoWeb").val() == "") $("#pagoWeb").val(0);
    if ($("#importe").val() == "") $("#import").val(0);
    if ($("#ValorPagado").val() == "") $("#ValorPagado").val(0);
    }

    $("#selectProduct").on("change", function () {
        var productId = $(this).val();
        if (!productId) return;

        //Busco el producto seleccionado
        var prodSelect;
        for (var i = 0; i < productos.length; i++) {
            if (productos[i].productId == productId) {
                prodSelect = productos[i];
                break;
            }
        }

        //Verifico que se haya añadido a la tabla
        var tabla = $('#tblProductos');
        var find = tabla.find('[data-id="' + productId + '"]');
        if (find.length == 0) {
            var desc = prodSelect.productName;

            addProductBodegaToTable(1, desc, prodSelect.productId, prodSelect.quantity);
            updatePrecio();
        }
        else {
            toastr.warning("El producto " + prodSelect.productName + " ya se añadió a la tabla");
        }
        $("#selectProduct").val("").trigger("change");
    });

    var cargarproductos = function () {
        $.ajax({
            type: "GET",
            url: "/BodegaProducto/GetProductExceptCombos",
            async: true,
            success: function (data) {
                productos = data; //Guardo los productos en una variable global

                $('#selectProduct').empty();
                $('#selectProduct').append(new Option("Seleccione los productos", "", true));
                if (data.length != 0) {
                    for (var i = 0; i < data.length; i++) {
                        var x = new Option(data[i].productName, data[i].productId);
                        x.title = data[i].description ?? '';
                        $('#selectProduct').append(x);
                    }
                }
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.statusText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.statusText);
            }
        });
    };

    var addProductBodegaToTable = function (cant, descripcion, id, maxcant) {
        //Get the reference of the Table's TBODY element.
        var tBody = $("#tblProductos > TBODY")[0];

        var index = $(tBody).children().length;

        var row = tBody.insertRow(index);


        var cell1 = $(row.insertCell(-1));
        cell1.append(cant);

        var cell2 = $(row.insertCell(-1));
        cell2.append(descripcion);

        var cell3 = $(row.insertCell(-1));
        cell3[0].style.display = "inline-flex";
        var btnView = $("<button type='button' data-id='" + id + "' class='btn btn-blue' title='View' style='font-size: 10px;padding:9px;margin-right:1px;'><i class='fa fa-eye'></i></button>");
        var btnEdit = $("<button type='button' data-id='" + id + "' class='btn btn-warning' title='Editar' style='font-size: 10px;padding:9px'><i class='fa fa-pencil'></i></button>");
        var btnRemove = $("<button type='button' data-id='" + id + "' class='btn btn-danger pull-right' title='Eliminar' style='font-size: 10px;padding:9px'><i class=' fa fa-close'></button>");
        var btnConfirm = $("<button type='button' data-id='" + id + "' class='btn btn-success hidden' title='Confirmar' style='font-size: 10px;padding:9px'><i class='fa fa-check'></button>");

        var i_id = $("<input type='hidden' value = '" + id + "' name='Products[" + index + "].ProductId'/>");
        var i_cant = $("<input type='hidden' value = '" + cant + "' name='Products[" + index + "].Cantidad'/>");

        btnView.on("click", function () {
            //Busco el producto y cargo su descripcion en un modal
            for (var i = 0; i < productos.length; i++) {
                if (productos[i].productId == id) {
                    //Actualizo la descripción
                    $('#nombreProducto').html(productos[i].productName);
                    $('#descripcion').html(productos[i].description);
                    $('#modalDescripcion').modal().show();
                }
            }
        });
        btnEdit.on("click", function () {
            cell1.html("<input type='number' data-id='" + id + "' max='" + maxcant + "' name='cell1' class='form-control' value='" + cell1.html() + "'/>");
            btnConfirm.removeClass("hidden");
            btnEdit.addClass("hidden");
            btnRemove.addClass("hidden");
            updatePrecio();
        });
        btnRemove.on("click", function () {
            row.remove();
            updatePrecio();
        });
        btnConfirm.on("click", function () {
            if (validateEditProduct()) {
                var parent = $(this).parent().parent();
                var elemcell1 = parent.find('[name = "cell1"]');
                var maxval = parseFloat(elemcell1.attr('max'));
                var value = parseFloat(elemcell1.val());
                if (value <= maxval) {
                    cell1.html($("[name='cell1']").val());
                    $(i_cant).val(value);
                    btnConfirm.addClass("hidden");
                    btnEdit.removeClass("hidden");
                    btnRemove.removeClass("hidden");
                }
                else {
                    toastr.warning("La cantidad máxima para ese campo es de " + maxval);
                }
                updatePrecio();
            }
        });
        cell3.append(btnView.add(btnEdit).add(btnRemove).add(btnConfirm));
        cell3.append(i_id);
        cell3.append(i_cant);
    };

    var updatePrecio = function () {
        var tBody = $("#tblProductos > TBODY");
        var precio = 0;
        for (var i = 0; i < productos.length; i++) {
            var id = productos[i].productId;
            var buscar = tBody.find('[data-id="' + id + '"]');
            if (buscar.length > 0) {
                var tr = buscar.parent().parent();
                var cant = parseFloat(tr[0].firstChild.innerHTML);
                precio += cant * parseFloat(productos[i].salePrice);
            }
        }
        precioProductos = precio;
        $("#importe").val(precioProductos + price);
        calculate();
    };

    var validateEditProduct = function () {
        if ($("[name='cell1']").val() == "" || $("[name='cell1']").val() == 0) {
            showWarningMessage("Atención", "El campo Cantidad no puede estar vacío.");
            return false;
        }
        if ($("[name='cell0']").val() == "") {
            showWarningMessage("Atención", "El campo Descripción no puede estar vacío.");
            return false;
        }
        return true;
    };

    cargarproductos();
});
