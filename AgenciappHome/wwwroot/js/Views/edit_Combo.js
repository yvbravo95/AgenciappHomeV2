var selected_municipio;
var productos = new Array();
var addcosto = 0;
var addprecio = 0;
var countEditProduct = 0;
var priceProductosBodega = 0;
var preciosTabla = 0;
var preciosBolsas = 0;

var validateAddProduct = function () {
  if ($("#newCant").val() == "") {
    showWarningMessage("Atención", "El campo Cantidad no puede estar vacío.");
    return false;
  }
  if ($("#newDescrip").val() == "") {
    showWarningMessage(
      "Atención",
      "El campo Descripción no puede estar vacío."
    );
    return false;
  }

  return true;
};

var validateEditProduct = function () {
  if ($("[name='cell1']").val() == "") {
    showWarningMessage("Atención", "El campo Cantidad no puede estar vacío.");
    return false;
  } else if ($("[name='cell5']").val() == "") {
    showWarningMessage("Atención", "El campo Tipo no puede estar vacío.");
    return false;
  }
  return true;
};

$(document).ready(function () {
  function block($this) {
    var block_ele = $this.closest(".card");
    block_ele.block({
      message:
        '<div id="load" class="ft-refresh-cw icon-spin font-medium-2"></div>',
      overlayCSS: {
        backgroundColor: "#FFF",
        cursor: "wait",
      },
      css: {
        border: 0,
        padding: 0,
        backgroundColor: "none",
      },
    });
  }

  function unblock($this) {
    var block_ele = $this.closest(".card");
    block_ele.unblock();
  }

  function selectMayorista() {
    // Para el select mayorista segun categoria
    var selectedcategory = $('[name = "orderType"][checked]');
    if (selectedcategory.val() == "CO") {
      $("#textDescripcion").html("");
      getMayorista("Combos");
    } else if (selectedcategory.val() == "CA" && agencyname == "Rey Envios") {
      getMayorista("Cantidad");
    } else {
      getMayorista("Paquete Aereo");
    }
  }

  //Guardo en esta variable si existe un mayorista por transferencia
  var idMayoristabyTransferencia = "";

  //Para cargar los datos del mayorista
  $(document).on("change", "#selectMayorista", function () {
    var id = $(this).val();
    var nombre = $('option[value="' + $(this).val() + '"]').html();
    // Si existe un mayorista por transferencia envio una alerta
    if (idMayoristabyTransferencia != "" && idMayoristabyTransferencia != id) {
      swal(
        {
          title: "Está usted seguro",
          text:
            "Esta seguro que desea cambiar al mayorista por transferencia a " +
            nombre,
          type: "warning",
          showCancelButton: true,
          confirmButtonColor: "#DA4453",
          confirmButtonText: "Si, cambiar",
          cancelButtonText: "No, mantener",
          closeOnConfirm: false,
          closeOnCancel: false,
        },
        function (isConfirm) {
          if (isConfirm) {
            idMayoristabyTransferencia = "";
            swal.close();
            aux(id);
          } else {
            $("#selectMayorista")
              .val(idMayoristabyTransferencia)
              .trigger("change");

            swal.close();
          }
        }
      );
    } else {
      aux(id);
    }
  });

  function aux(idmayorista) {
    $.ajax({
      type: "POST",
      url: "/OrderNew/getMayorista",
      data: {
        id: idmayorista,
      },
      async: false,
      success: function (data) {
        $("#select2-selectMayorista-container").attr(
          "title",
          data.termsConditions
        );
        $("#textDescripcion").html(data.termsConditions);

        getprecioxlibra();
      },
      failure: function (response) {
        showErrorMessage("ERROR", response.responseText);
      },
      error: function (response) {
        showErrorMessage("ERROR", response.responseText);
      },
    });
  }

  function getMayorista(category) {
    $.ajax({
      type: "POST",
      url: "/OrderNew/getMayoristas",
      data: {
        category: category,
      },
      async: false,
      success: function (data) {
        idMayoristabyTransferencia = "";
        $("[name='selectMayorista']").val("").trigger("change");
        $("#selectMayorista").empty();
        $("#selectMayorista").append(
          new Option("Seleccione un mayorista", "", true)
        );

        var categorySelected = $('[name = "orderType"][checked]').val();
        if (data.length != 0) {
          for (var i = 0; i < data.length; i++) {
            $("[name='selectMayorista']").append(
              new Option(data[i].name, data[i].idWholesaler)
            );
            if (data[i].byTransferencia) {
              idMayoristabyTransferencia = data[i].idWholesaler;
              $("[name='selectMayorista']")
                .val(idMayoristabyTransferencia)
                .trigger("change");
              aux(idMayoristabyTransferencia);
            }
          }
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

  /**********************************
   *   Form Wizard Step Icon
   **********************************/
  $(".step-icon").each(function () {
    var $this = $(this);
    if ($this.siblings("span.step").length > 0) {
      $this.siblings("span.step").empty();
      $(this).appendTo($(this).siblings("span.step"));
    }
  });

  $("#zc").steps({
    headerTag: "h6",
    bodyTag: "fieldset",
    transitionEffect: "fade",
    titleTemplate: '<span class="step">#index#</span> #title#',
    labels: {
      previous: "Anterior",
      next: "Siguiente",
      finish: "Editar orden",
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
        if (
          $("#single-placeholder-2").val() == "" ||
          $("#single-placeholder-3").val() == ""
        ) {
          return false;
        }

        if ($("#inputClientMovil").val() == "") {
          showWarningMessage(
            "Atención",
            "El campo de teléfono del cliente es obligatorio."
          );
          return false;
        }
        if ($("#provincia").val() == null) {
          showWarningMessage(
            "Atención",
            "El campo de provincia del contacto es obligatorio."
          );
          return false;
        }
      }

      //----------------Step2

      if (newIndex == 2) {
        if ($("#txtNameVA").val() == "") return false;
        else if ($("#tblProductos > TBODY")[0].rows.length == 1) {
          return false;
        } else {
          //Preparando el step3 con los productos seleccinados
          var listProduct = new Array();

          var tBody = $("#tblProductos > TBODY")[0];

          for (var i = 0; i < tBody.rows.length - 1; i++) {
            var fila = tBody.rows[i];
            listProduct[i] = new Array();

            listProduct[i]["cantidad"] = $(fila.children[0]).html();
            listProduct[i]["descripcion"] = $(fila.children[1]).html();

            var chk =
              '<label class="custom-control custom-checkbox">\n' +
              '                                    <input type="checkbox" class="custom-control-input order-select" id="chk' +
              i +
              '" />\n' +
              '                                    <span class="custom-control-indicator"></span>\n' +
              '                                    <span class="custom-control-description"></span>\n' +
              "                                </label>";

            var tr =
              "<tr><td>" +
              chk +
              "</td><td>" +
              '<input class="form-control" style="width: 85px" type="number" min="1" max="' +
              listProduct[i]["cantidad"] +
              '" value="1">' +
              "</td><td>" +
              listProduct[i]["cantidad"] +
              "</td><td>" +
              listProduct[i]["descripcion"] +
              "</td></tr>";

            $("#tableBody").html($("#tableBody").html() + tr);
          }
        }
      }
      return true;
    },
    onFinishing: function (event, currentIndex) {
      return EditOrder();
    },
    onFinished: function (event, currentIndex) { },
  });

  $("a[href='#next']").addClass("hidden");
  $("a[href=#previous]").hide();
  $("a[href=#previous]").click(function () {
    $("#showAllContacts").removeAttr("disabled");
    $("a[href=#previous]").hide();
  });
  $("a[href=#next]").click(function () {
    $("#showAllContacts").attr("disabled", "disabled");

    $("a[href=#previous]").show();
  });

  /*********************************************/

  //placeholder cliente y contacto
  $("#municipio").select2({});

  $(".select2-placeholder-selectClient").select2({
    placeholder: "Buscar cliente por teléfono, nombre o apellido",
    val: null,
    ajax: {
        type: "POST",
        dataType: "json",
        delay: 500,
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

  $(".select2-placeholder-selectContact").select2({
    placeholder: "Buscar contacto por teléfono, nombre o apellido",
    text: " ",
    ajax: {
      type: "POST",
      url: "/Contacts/findContacts",
      data: function (params) {
        var query = {
          search: params.term,
          idClient: $(".select2-placeholder-selectClient").val(),
        };

        // Query parameters will be ?search=[term]&type=public
        return query;
      },
      processResults: function (data) {
        // Transforms the top-level key of the response object from 'items' to 'results'
        return {
          results: $.map(data, function (obj) {
            return {
              id: obj.contactId,
              text: obj.phone1 + "-" + obj.name + " " + obj.lastName,
            };
          }),
        };
      },
    },
  });

  $("#selectMayorista").select2({
    placeholder: "Seleccione un mayorista",
    val: null,
  });

  $(".hide-search-clientState").select2({
    minimumResultsForSearch: Infinity,
    placeholder: "Estado",
  });

  $(".hide-search-contactCity2").select2({
    minimumResultsForSearch: Infinity,
    placeholder: "Provincia",
  });

  $("#inputClientMovil").mask("(000)-000-0000", {
    placeholder: "(000)-000-0000",
  });

  $("#inputContactPhoneMovil").mask("(000)-000-0000", {
    placeholder: "(000)-000-0000",
  });

  $("#contactCI").mask("00000000000", {
    placeholder: "Carnet de Identidad",
  });

  $("#inputContactPhoneHome").mask("(000)-000-0000", {
    placeholder: "(000)-000-0000",
  });

  //placeholder orden

  $(".hide-search-pago").select2({
    minimumResultsForSearch: Infinity,
    placeholder: "Tipo de Pago",
  });

  $(".select2-placeholder-valor").select2({
    placeholder: "Valor Aduanal",
  });

  $(".select2-placeholder-selectProduct").select2({
    placeholder: "Buscar producto por tipo, color, talla o marca",
  });
  /**********Clientes y Contactos ***********/
  var selectMunicipios = function () {
    var provincia = $("#provincia").val();
    if (!provincia) return;

    $.ajax({
      url: "/Provincias/Municipios?nombre=" + provincia,
      type: "POST",
      dataType: "json",
      success: function (response) {
        var municipios = $("#municipio");
        municipios.empty();
        municipios.append(new Option());
        for (var i in response) {
          var m = response[i];
          municipios.append(new Option(m, m));
        }
        municipios.val(selected_municipio).trigger("change");
      },
    });
  };

  var showClient = function () {
    var value = $(".select2-placeholder-selectClient").val();
    if (value) {
      selectedClient = value;
    }
    $.ajax({
      type: "POST",
      url: "/OrderNew/GetClient",
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

        //Datos del Cliente en Step 1
        $("#inputClientName").val(data.name);
        $("#inputClientLastName").val(data.lastName);
        $("#inputClientMovil").val(data.movil);
        $("#inputClientEmail").val(data.email);
        $("#inputClientAddress").val(data.calle);
        $("#inputClientCity").val(data.city);
        $("#inputClientZip").val(data.zip);
        $("#inputClientState").val(data.state).trigger("change");
        $("#remitente").html(data.name + " " + data.lastName);

        showContactsOfAClient();
      },
      failure: function (response) {
        showErrorMessage("ERROR", response.statusText);
      },
      error: function (response) {
        showErrorMessage("ERROR", response.statusText);
      },
    });
  };

  var showContact = function () {
    var idContacto = $(".select2-placeholder-selectContact").val();
    if (idContacto == null || idContacto == "") {
      idContacto = selectedContact;
    }
    if (idContacto != "00000000-0000-0000-0000-000000000000") {
      $.ajax({
        type: "POST",
        url: "/OrderNew/GetContact",
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        data: JSON.stringify(idContacto),
        async: false,
        success: function (data) {
          $("#inputContactName").val(data.name);
          $("#inputContactLastName").val(data.lastName);
          $("#inputContactPhoneMovil").val(data.movilPhone);
          $("#inputContactPhoneHome").val(data.casaPhone);
          $("#contactDireccion").val(data.direccion);
          $("#provincia").val(data.city).trigger("change");
          $("#municipio").val(data.municipio);
          $("#reparto").val(data.reparto);
          $("#contactCI").val(data.ci);
          selected_municipio = data.municipio;

          $("#destinatario").html(data.name + " " + data.lastName);

          $("a[href='#next']").removeClass("hidden");
        },
        failure: function (response) {
          showErrorMessage("ERROR", response.statusText);
        },
        error: function (response) {
          showErrorMessage("ERROR", response.statusText);
        },
      });
      selectedContact = null;
    }
  };

  var showContactsOfAClient = function () {
    var idClient = $(".select2-placeholder-selectClient").val();
    $("#inputContactName").val("");
    $("#inputContactLastName").val("");
    $("#inputContactPhoneMovil").val("");
    $("#inputContactPhoneHome").val("");
    $("#contactDireccion").val("");
    $("#provincia").val("").trigger("change");
    $("#municipio").val("");
    $("#reparto").val("");
    $("#contactCI").val("");

    $.ajax({
      type: "GET",
      url: "/Contacts/GetContactsOfAClient?id=" + idClient,
      dataType: "json",
      contentType: "application/json",
      async: true,
      success: function (data) {
        $("[name='selectContact']").empty();
        $("[name='selectContact']").append(new Option());
        for (var i = 0; i < data.length; i++) {
          var contactData;
          if (data[i].phone1 != "")
            contactData =
              data[i].phone1 + " - " + data[i].name + " " + data[i].lastName;
          else
            contactData =
              data[i].phone2 + " - " + data[i].name + " " + data[i].lastName;
          $("[name='selectContact']").append(
            new Option(contactData, data[i].contactId)
          );
        }
        var last = data[data.length - 1];

        if (selectedContact != null)
          $("[name='selectContact']").val(selectedContact).trigger("change");
        else $("[name='selectContact']").val(last.contactId).trigger("change");

        showContact();
        $("#editarContacto, a[href='#next']").removeClass("hidden");
      },
      failure: function (response) {
        showErrorMessage("ERROR", response.responseText);
      },
      error: function (response) {
        showErrorMessage("ERROR", response.responseText);
      },
    });
  };

  var mostrarEstados = function () {
    $("#inputClientState").empty();
    $("#inputClientState").append(new Option());
    $("#inputClientState").append(new Option("Alabama", "Alabama"));
    $("#inputClientState").append(new Option("Alaska", "Alaska"));
    $("#inputClientState").append(
      new Option("American Samoa", "American Samoa")
    );
    $("#inputClientState").append(new Option("Arizona", "Arizona"));
    $("#inputClientState").append(new Option("Arkansas", "Arkansas"));
    $("#inputClientState").append(
      new Option("Armed Forces Americas", "Armed Forces Americas")
    );
    $("#inputClientState").append(
      new Option(
        "Armed Forces Europe, Canada, Africa and Middle East",
        "Armed Forces Europe, Canada, Africa and Middle East"
      )
    );
    $("#inputClientState").append(
      new Option("Armed Forces Pacific", "Armed Forces Pacific")
    );
    $("#inputClientState").append(new Option("California", "California"));
    $("#inputClientState").append(new Option("Colorado", "Colorado"));
    $("#inputClientState").append(new Option("Connecticut", "Connecticut"));
    $("#inputClientState").append(new Option("Delaware", "Delaware"));
    $("#inputClientState").append(
      new Option("District of Columbia", "District of Columbia")
    );
    $("#inputClientState").append(new Option("Florida", "Florida"));
    $("#inputClientState").append(new Option("Georgia", "Georgia"));
    $("#inputClientState").append(new Option("Guam", "Guam"));
    $("#inputClientState").append(new Option("Hawaii", "Hawaii"));
    $("#inputClientState").append(new Option("Idaho", "Idaho"));
    $("#inputClientState").append(new Option("Illinois", "Illinois"));
    $("#inputClientState").append(new Option("Indiana", "Indiana"));
    $("#inputClientState").append(new Option("Iowa", "Iowa"));
    $("#inputClientState").append(new Option("Kansas", "Kansas"));
    $("#inputClientState").append(new Option("Kentucky", "Kentucky"));
    $("#inputClientState").append(new Option("Louisiana", "Louisiana"));
    $("#inputClientState").append(new Option("Maine", "Maine"));
    $("#inputClientState").append(
      new Option("Marshall Islands", "Marshall Islands")
    );
    $("#inputClientState").append(new Option("Maryland", "Maryland"));
    $("#inputClientState").append(new Option("Massachusetts", "Massachusetts"));
    $("#inputClientState").append(new Option("Michigan", "Michigan"));
    $("#inputClientState").append(new Option("Micronesia", "Micronesia"));
    $("#inputClientState").append(new Option("Minnesota", "Minnesota"));
    $("#inputClientState").append(new Option("Mississippi", "Mississippi"));
    $("#inputClientState").append(new Option("Missouri", "Missouri"));
    $("#inputClientState").append(new Option("Montana", "Montana"));
    $("#inputClientState").append(new Option("Nebraska", "Nebraska"));
    $("#inputClientState").append(new Option("Nevada", "Nevada"));
    $("#inputClientState").append(new Option("New Hampshire", "New Hampshire"));
    $("#inputClientState").append(new Option("New Jersey", "New Jersey"));
    $("#inputClientState").append(new Option("New Mexico", "New Mexico"));
    $("#inputClientState").append(new Option("New York", "New York"));
    $("#inputClientState").append(
      new Option("North Carolina", "North Carolina")
    );
    $("#inputClientState").append(new Option("North Dakota", "North Dakota"));
    $("#inputClientState").append(
      new Option("Northern Mariana Islands", "Northern Mariana Islands")
    );
    $("#inputClientState").append(new Option("Ohio", "Ohio"));
    $("#inputClientState").append(new Option("Oklahoma", "Oklahoma"));
    $("#inputClientState").append(new Option("Oregon", "Oregon"));
    $("#inputClientState").append(new Option("Palau", "Palau"));
    $("#inputClientState").append(new Option("Pennsylvania", "Pennsylvania"));
    $("#inputClientState").append(new Option("Puerto Rico", "Puerto Rico"));
    $("#inputClientState").append(new Option("Rhode Island", "Rhode Island"));
    $("#inputClientState").append(
      new Option("South Carolina", "South Carolina")
    );
    $("#inputClientState").append(new Option("South Dakota", "South Dakota"));
    $("#inputClientState").append(new Option("Tennessee", "Tennessee"));
    $("#inputClientState").append(new Option("Texas", "Texas"));
    $("#inputClientState").append(new Option("Utah", "Utah"));
    $("#inputClientState").append(new Option("Vermont", "Vermont"));
    $("#inputClientState").append(
      new Option("Virgin Islands", "Virgin Islands")
    );
    $("#inputClientState").append(new Option("Virginia", "Virginia"));
    $("#inputClientState").append(new Option("Washington", "Washington"));
    $("#inputClientState").append(new Option("West Virginia", "West Virginia"));
    $("#inputClientState").append(new Option("Wisconsin", "Wisconsin"));
    $("#inputClientState").append(new Option("Wyoming", "Wyoming"));
  };

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
    $("#nuevoContacto").removeAttr("disabled");
    $(".select2-placeholder-selectContact").removeAttr("disabled");
    $("#editarContacto").removeAttr("disabled");

    $("#inputClientName").attr("disabled", "disabled");
    $("#inputClientLastName").attr("disabled", "disabled");
    $("#inputClientMovil").attr("disabled", "disabled");
    $("#inputClientEmail").attr("disabled", "disabled");
    $("#inputClientAddress").attr("disabled", "disabled");
    $("#inputClientCity").attr("disabled", "disabled");
    $("#inputClientState").attr("disabled", "disabled");
    $("#inputClientZip").attr("disabled", "disabled");

    $("#editarCliente").removeClass("hidden");
    $("#cancelarCliente").addClass("hidden");
    $("#guardarCliente").addClass("hidden");

    if ($("#inputContactName").val() != null) {
      $("a[href='#next']").removeClass("hidden");
    }
    $("#showAllContacts").removeAttr("disabled");
  };

  var cancelClientForm = function () {
    $("#inputClientName").val($("#inputClientName").data("prevVal"));
    $("#inputClientLastName").val($("#inputClientLastName").data("prevVal"));
    $("#inputClientMovil").val($("#inputClientMovil").data("prevVal"));
    $("#inputClientEmail").val($("#inputClientEmail").data("prevVal"));
    $("#inputClientAddress").val($("#inputClientAddress").data("prevVal"));
    $("#inputClientCity").val($("#inputClientCity").data("prevVal"));
    $("#inputClientState")
      .val($("#inputClientState").data("prevVal"))
      .trigger("change");
    $("#inputClientZip").val($("#inputClientZip").data("prevVal"));
    desactClientForm();
  };

  var validateEditarContacto = function () {
    if ($("#inputContactName").val() == "") {
      showWarningMessage("Atención", "El campo Nombre no puede estar vacío.");
      return false;
    } else if ($("#inputContactLastName").val() == "") {
      showWarningMessage(
        "Atención",
        "El campo Apellidos no puede estar vacío."
      );
      return false;
    } else if (
      $("#inputContactPhoneMovil").val() == "" &&
      $("#inputContactPhoneHome").val() == ""
    ) {
      showWarningMessage(
        "Atención",
        "Debe introducir al menos un teléfono de contacto."
      );
      return false;
    } else if ($("#contactCI").val().length > 0) {
      if ($("#contactCI").val().length != 11) {
        showWarningMessage(
          "Atención",
          "El carnet de identidad debe tener 11 dígitos"
        );
        return false;
      }
    } else if ($("#contactDireccion").val() == "") {
      showWarningMessage("Atención", "El campo Dirección no puede estar vacío");
      return false;
    } else if ($("#provincia").val() == "") {
      showWarningMessage(
        "Atención",
        "El campo Provincia no puede estar vacío."
      );
      return false;
    } else if ($("#municipio").val() == "") {
      showWarningMessage(
        "Atención",
        "El campo Municipio no puede estar vacío."
      );
      return false;
    }
    return true;
  };

  var desactContactForm = function () {
    $("#nuevoCliente").removeAttr("disabled");
    $(".select2-placeholder-selectClient").removeAttr("disabled");
    $("#nuevoContacto").removeAttr("disabled");
    $(".select2-placeholder-selectContact").removeAttr("disabled");
    $("#showAllContacts").removeAttr("disabled");
    $("#editarCliente").removeAttr("disabled");

    $("#inputContactName").attr("disabled", "disabled");
    $("#inputContactLastName").attr("disabled", "disabled");
    $("#inputContactPhoneMovil").attr("disabled", "disabled");
    $("#inputContactPhoneHome").attr("disabled", "disabled");
    $("#contactDireccion").attr("disabled", "disabled");
    $("#provincia").attr("disabled", "disabled");
    $("#municipio").attr("disabled", "disabled");
    $("#reparto").attr("disabled", "disabled");
    $("#contactCI").attr("disabled", "disabled");

    $("#editarContacto").removeClass("hidden");
    $("#cancelarContacto").addClass("hidden");
    $("#guardarContacto").addClass("hidden");

    $("a[href='#next']").removeClass("hidden");
    $("#showAllContacts").attr("disabled", "disabled");
  };

  var cancelarContactForm = function () {
    $("#inputContactName").val($("#inputContactName").data("prevVal"));
    $("#inputContactLastName").val($("#inputContactLastName").data("prevVal"));
    $("#inputContactPhoneMovil").val(
      $("#inputContactPhoneMovil").data("prevVal")
    );
    $("#inputContactPhoneHome").val(
      $("#inputContactPhoneHome").data("prevVal")
    );
    $("#contactDireccion").val($("#contactDireccion").data("prevVal"));
    $("#provincia").val($("#provincia").data("prevVal")).trigger("change");
    $("#reparto").val($("#reparto").data("prevVal"));
    $("#municipio").val($("#municipio").data("prevVal"));
    $("#contactCI").val($("#contactCI").data("prevVal"));

    desactContactForm();
  };

  $("#provincia").on("change", selectMunicipios);

  $(".select2-placeholder-selectClient").on("select2:select", function (a, b) {
    $(".select2-placeholder-selectContact").removeAttr("disabled");
    $("#nuevoContacto").removeAttr("disabled");
    $("#editarCliente").removeClass("hidden");
    $(".select2-placeholder-selectContact").empty();
    $("a[href='#next']").addClass("hidden");
    showClient();
  });

  $(".select2-placeholder-selectContact").on("select2:select", function () {
    $("#editarContacto").removeClass("hidden hide-search-contactCity");
    showContact();
  });

  //se activan los campos desactivados y se muestran los botones guardar y cancelar
  $("#editarCliente").on("click", function () {
    // para que no pueda crear nuevo cliente mientras edita cliente
    $("#nuevoCliente").attr("disabled", "disabled");

    // para que no pueda cambiar de cliente mientras edita cliente
    $(".select2-placeholder-selectClient").attr("disabled", "disabled");

    // para que no pueda crear nuevo contacto mientras edita cliente
    $("#nuevoContacto").attr("disabled", "disabled");

    // para que no pueda cambiar de contacto mientras edita cliente
    $(".select2-placeholder-selectContact").attr("disabled", "disabled");

    // para que no pueda editar contacto mientras edita cliente
    $("#editarContacto").attr("disabled", "disabled");

    $("a[href='#next']").addClass("hidden");

    $("#showAllContacts").attr("disabled", "disabled");

    $("#inputClientName")
      .removeAttr("disabled")
      .data("prevVal", $("#inputClientName").val());
    $("#inputClientLastName")
      .removeAttr("disabled")
      .data("prevVal", $("#inputClientLastName").val());
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

    $("#editarCliente").addClass("hidden");
    $("#cancelarCliente").removeClass("hidden");
    $("#guardarCliente").removeClass("hidden");
  });

  $("#cancelarCliente").click(cancelClientForm);

  $("#guardarCliente").on("click", function () {
    if (validateEditarCliente()) {
      var source = [
        $(".select2-placeholder-selectClient").val(),
        $("#inputClientName").val(),
        $("#inputClientLastName").val(),
        $("#inputClientEmail").val(),
        $("#inputClientMovil").val(),
        $("#inputClientAddress").val(),
        $("#inputClientCity").val(),
        $("#inputClientState").val(),
        $("#inputClientZip").val(),
        "",
        "",
        "",
      ];
      $.ajax({
        type: "POST",
        url: "/Clients/EditClient",
        data: JSON.stringify(source),
        dataType: "json",
        contentType: "application/json",
        async: false,
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

  //se activan los campos desactivados y se muestran los botones guardar y cancelar
  $("#editarContacto").on("click", function () {
    // para que no pueda crear nuevo cliente mientras edita contacto
    $("#nuevoCliente").attr("disabled", "disabled");

    // para que no pueda cambiar de cliente mientras edita contacto
    $(".select2-placeholder-selectClient").attr("disabled", "disabled");

    // para que no pueda crear nuevo contacto mientras edita contacto
    $("#nuevoContacto").attr("disabled", "disabled");

    // para que no pueda cambiar de contacto mientras edita contacto
    $(".select2-placeholder-selectContact").attr("disabled", "disabled");

    // para que no pueda cambiar de contacto mientras edita contacto
    $("#showAllContacts").attr("disabled", "disabled");

    // para que no pueda editar cliente mientras edita contacto
    $("#editarCliente").attr("disabled", "disabled");

    // para que no pueda avanzar a la otra parte del formulario
    $("a[href='#next']").addClass("hidden");

    $("#inputContactName")
      .removeAttr("disabled")
      .data("prevVal", $("#inputContactName").val());
    $("#inputContactLastName")
      .removeAttr("disabled")
      .data("prevVal", $("#inputContactLastName").val());
    $("#inputContactPhoneMovil")
      .removeAttr("disabled")
      .data("prevVal", $("#inputContactPhoneMovil").val());
    $("#inputContactPhoneHome")
      .removeAttr("disabled")
      .data("prevVal", $("#inputContactPhoneHome").val());
    $("#contactDireccion")
      .removeAttr("disabled")
      .data("prevVal", $("#contactDireccion").val());
    $("#provincia")
      .removeAttr("disabled")
      .data("prevVal", $("#provincia").val());
    $("#municipio")
      .removeAttr("disabled")
      .data("prevVal", $("#municipio").val());
    $("#reparto").removeAttr("disabled").data("prevVal", $("#reparto").val());
    $("#contactCI")
      .removeAttr("disabled")
      .data("prevVal", $("#contactCI").val());

    $("#editarContacto").addClass("hidden");
    $("#cancelarContacto").removeClass("hidden");
    $("#guardarContacto").removeClass("hidden");
  });

  $("#cancelarContacto").click(cancelarContactForm);

  $("#guardarContacto").on("click", function () {
    selectedContact = $(".select2-placeholder-selectContact").val();
    if (validateEditarContacto()) {
      var source = [
        $(".select2-placeholder-selectContact").val(),
        $("#inputContactName").val(),
        $("#inputContactLastName").val(),
        $("#inputContactPhoneMovil").val(),
        $("#inputContactPhoneHome").val(),
        $("#contactDireccion").val(),
        $("#provincia").val(),
        $("#municipio").val(),
        $("#reparto").val(),
        $("#contactCI").val(),
        $(".select2-placeholder-selectClient").val(),
      ];

      $.ajax({
        type: "POST",
        url: "/Contacts/EditContact",
        data: JSON.stringify(source),
        dataType: "json",
        contentType: "application/json",
        async: false,
        success: function () {
          showOKMessage("Editar Contacto", "Contacto editado con éxito");
        },
        failure: function (response) {
          showErrorMessage("ERROR", response.responseText);
        },
        error: function (response) {
          showErrorMessage("ERROR", response.responseText);
        },
      });

      desactContactForm();
    }
  });

  //$("#nuevoContacto").attr('disabled', 'disabled');

  mostrarEstados();
  showClient();

  /***********DATOS DE LA ORDEN *************/

  var setNoOrden = function () {
    var time = $("#time").html();
    $("#no_orden").html("CO" + time);
  };

  var calcMoney = function () {
    if (blockRight == "True") {
      calcMoneyBlockRight();
    } else {
      var productShipping = parseFloat($('#ProductsShipping').val())
      var cantLb = $("#CantLb").val();
      var precioLb = $("#PriceLb").val();
      var oCargos = $("#OtrosCostos").val();
      var delivery = $('[name="Delivery"]').val();
      var precioTotalValue =
          cantLb * precioLb + parseFloat(oCargos) + parseFloat(delivery) + productShipping;

      if ($("#checkpago").is(":checked")) {
        var pagoCash = parseFloat($("#pagoCash").val());
        var pagoZelle = parseFloat($("#pagoZelle").val());
        var pagoCheque = parseFloat($("#pagoCheque").val());
        var pagoTransf = parseFloat($("#pagoTransf").val());
        var pagoWeb = parseFloat($("#pagoWeb").val());

        var pagoCrDeb = parseFloat($("#pagoCredito").val());
        var pagoCrDebReal =
          parseFloat($("#pagoCredito").val()) /
          (1 + parseFloat($("#fee").html()) / 100);
        var feeCrDeb = pagoCrDeb - pagoCrDebReal;
        if (pagoCrDeb > 0) {
          $("#contfee").show();
        } else {
          $("#contfee").hide();
        }
        precioTotalValue += feeCrDeb;

        var pagoCredito = 0;
        if ($("#check_credito").is(":checked")) {
          pagoCredito = parseFloat($("#credito").html()).toFixed(2);
          pagoCredito =
            pagoCredito > precioTotalValue ? precioTotalValue : pagoCredito;
        }

        var balanceValue =
          precioTotalValue -
          pagoCash -
          pagoZelle -
          pagoCheque -
          pagoCrDeb -
          pagoTransf -
          pagoWeb -
          pagoCredito;

        $("#pagoCash").attr("max", (balanceValue + pagoCash).toFixed(2));
        $("#pagoZelle").attr("max", (balanceValue + pagoZelle).toFixed(2));
        $("#pagoCheque").attr("max", (balanceValue + pagoCheque).toFixed(2));
        $("#pagoTransf").attr("max", (balanceValue + pagoTransf).toFixed(2));
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
        $('[name = "AuthSaleAmount"]').val(pagoCrDebReal.toFixed(2));
        var aconvcharge = parseFloat($('[name = "AuthConvCharge"]').val());
        var total = pagoCrDebReal + (pagoCrDebReal * aconvcharge) / 100;
        $('[name = "TotalCharge"]').val(total.toFixed(2));
      } else {
        tipoPagoId = $('[name = "TipoPago"]').val();
        tipoPago = $('option[value = "' + tipoPagoId + '"]').html();

        if (tipoPago == "Crédito o Débito") {
          var fee = parseFloat($("#fee").html());
          //Valor Sale Amount en authorization card
          $('[name = "AuthSaleAmount"]').val(precioTotalValue.toFixed(2));
          precioTotalValue = precioTotalValue + precioTotalValue * (fee / 100);
          $('[name = "TotalCharge"]').val(precioTotalValue.toFixed(2));
        }
        var balanceValue = 0;
        if ($("#check_credito").is(":checked")) {
          if (
            precioTotalValue.toFixed(2) - parseFloat($("#credito").html()) >
            0
          ) {
            $("#ValorPagado").attr(
              "max",
              precioTotalValue.toFixed(2) - parseFloat($("#credito").html())
            );
            $("#ValorPagado").val(
              (
                precioTotalValue.toFixed(2) - parseFloat($("#credito").html())
              ).toFixed(2)
            );
          } else {
            $("#ValorPagado").attr("max", 0);
            $("#ValorPagado").val(0);
          }
        } else {
          $("#ValorPagado").attr("max", precioTotalValue.toFixed(2));
          $("#ValorPagado").val(precioTotalValue.toFixed(2));
        }
      }

      $("#precioTotalValue").html(precioTotalValue.toFixed(2));
      $("#balanceValue").html(balanceValue.toFixed(2));
      if ($("#balanceValue").html() == "-0.00") $("#balanceValue").html("0.00");
    }
  };

  var calcMoneyPayment = function () {
    if (blockRight == "True") {
      calcMoneyBlockRight();
    } else {
      var productShipping = parseFloat($('#ProductsShipping').val())
      var cantLb = $("#CantLb").val();
      var precioLb = $("#PriceLb").val();
      var oCargos = $("#OtrosCostos").val();
      var pagado = $("#ValorPagado").val();
      var delivery = $('[name="Delivery"]').val();

        var precioTotalValue = cantLb * precioLb + parseFloat(oCargos) + parseFloat(delivery) + productShipping;
      var max = precioTotalValue;

      if (tipoPago == "Crédito o Débito") {
        var fee = parseFloat($("#fee").html());
        var pagdoReal = pagado / (1 + fee / 100);
        var feeCrDeb = pagado - pagdoReal;
        precioTotalValue = precioTotalValue + feeCrDeb;
        max = max + (max * fee) / 100;

        //Valor Sale Amount en authorization card
        $('[name = "AuthSaleAmount"]').val(pagdoReal.toFixed(2));
        $('[name = "TotalCharge"]').val(pagado);
      }

      precioTotalValue = precioTotalValue.toFixed(2);
      var balanceValue = 0;
      if ($("#check_credito").is(":checked")) {
        var pagoCredito = parseFloat($("#credito").html()).toFixed(2);
        pagoCredito =
          pagoCredito > precioTotalValue ? precioTotalValue : pagoCredito;
        balanceValue = precioTotalValue - pagado - pagoCredito;
      } else {
        balanceValue = precioTotalValue - pagado;
      }
      $("#precioTotalValue").html(precioTotalValue);
      $("#balanceValue").html(balanceValue.toFixed(2));
      if ($("#balanceValue").html() == "-0.00") $("#balanceValue").html("0.00");
      $("#ValorPagado").attr("max", max.toFixed(2));
    }
  };

  var calcMoneyBlockRight = function () {
    var productShipping = parseFloat($('#ProductsShipping').val())
    var cantLb = $("#CantLb").val();
    var precioLb = $("#PriceLb").val();
    var oCargos = $("#OtrosCostos").val();
    var delivery = $('[name="Delivery"]').val();
    var precioTotalValue =
        cantLb * precioLb + parseFloat(oCargos) + feeReal + parseFloat(delivery) + productShipping;
    var balanceValue = precioTotalValue - valorPagado;

    $("#precioTotalValue").html(precioTotalValue.toFixed(2));
    $("#balanceValue").html(balanceValue.toFixed(2));
    if ($("#balanceValue").html() == "-0.00") $("#balanceValue").html("0.00");
    };

    function updatePrecioTabla() {
        var tBody = $("#tblProductos > TBODY");
        var precio = 0;
        for (var i = 0; i < productos.length; i++) {
            var id = productos[i].productId;
            var ttBody = $(tBody)[0];
            for (var j = 0; j < ttBody.rows.length; j++) {
                const row = ttBody.rows[j]
                const idRow = $(row).data('id');
                if (id == idRow) {
                    const qty = $(row)[0].childNodes[0].innerText;
                    precio += parseInt(qty) * parseFloat(productos[i].salePrice);
                }
            }
        }
        return precio;
    }

    function updatePrecio() {
        preciosTabla = updatePrecioTabla();
        var preciototal = preciosTabla + preciosBolsas;
        priceProductosBodega = preciototal;
        categorySelected = $('[name = "orderType"][checked]').val();
        if (categorySelected == "CO") {
            preciototal = preciototal + addprecio;
            $("#PriceLb").val(preciototal.toFixed(2));
        }

        calcMoney();
    }

    var addProductBodegaToTable = function (
        cant,
        descripcion,
        id,
        maxcant,
        wholesaler,
        wholesalerId,
        packageItemId
    ) {
        //Get the reference of the Table's TBODY element.
        var tBody = $("#tblProductos > TBODY")[0];

        var index = $(tBody).children().length - 1;

        ////Add Row.
        var row = tBody.insertRow(index);
        $(row).attr("data-wholesaler", wholesaler);
        $(row).attr("data-wholesalerId", wholesalerId);

        if (id != "" && id != null) {
            $(row).attr("style", "background-color: #ffc8004a;");
            $(row).attr("data-id", id);
            $(row).attr("shippingItem-id", packageItemId);
        }

        var cell1 = $(row.insertCell(-1));
        cell1.append(cant);

        var cell5 = $(row.insertCell(-1));
        cell5.append(descripcion);

        ////Add Button cell.
        var cell6 = $(row.insertCell(-1));
        cell6[0].style.display = "inline-flex";
        var btnView = $(
            "<button type='button' data-id='" +
            id +
            "' class='btn btn-blue' title='View' style='font-size: 10px;padding:9px;margin-right:1px;'><i class='fa fa-eye'></i></button>"
        );
        var btnEdit = $(
            "<button type='button' data-id='" +
            id +
            "' class='btn btn-warning' title='Editar' style='font-size: 10px;padding:9px'><i class='fa fa-pencil'></i></button>"
        );
        var btnRemove = $(
            "<button type='button' data-id='" +
            id +
            "' class='btn btn-danger pull-right' title='Eliminar' style='font-size: 10px;padding:9px'><i class=' fa fa-close'></button>"
        );
        var btnConfirm = $(
            "<button type='button' data-id='" +
            id +
            "' class='btn btn-success hidden' title='Confirmar' style='font-size: 10px;padding:9px'><i class='fa fa-check'></button>"
        );

        btnView.on("click", function () {
            //Busco el producto y cargo su descripcion en un modal
            for (var i = 0; i < productos.length; i++) {
                if (productos[i].productId == id) {
                    //Actualizo la descripción
                    $("#nombreProducto").html(productos[i].productName);
                    $("#descripcion").html(productos[i].description);
                    $("#modalDescripcion").modal().show;
                }
            }
        });
        btnEdit.on("click", function () {
            countEditProduct++;
            cell1.html(
                "<input type='number' data-id='" +
                id +
                "' max='" +
                maxcant +
                "' name='cell1' class='form-control' value='" +
                cell1.html() +
                "'/>"
            );
            cell5.html(
                "<input name='cell5' data-id='" +
                id +
                "' class='form-control' value='" +
                cell5.html() +
                "'/>"
            );

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
            countEditProduct--;

            if (validateEditProduct()) {
                var parent = $(this).parent().parent();
                var elemcell1 = parent.find('[name = "cell1"]');
                var maxval = parseFloat(elemcell1.attr("max"));
                var value = parseFloat(elemcell1.val());
                if (value <= maxval) {
                    cell1.html($("[name='cell1']").val());
                    cell5.html($("[name='cell5']").val());

                    btnConfirm.addClass("hidden");
                    btnEdit.removeClass("hidden");
                    btnRemove.removeClass("hidden");
                } else {
                    toastr.warning("La cantidad máxima para ese campo es de " + maxval);
                }
                updatePrecio();
            }
        });
        cell6.append(btnView.add(btnEdit).add(btnRemove).add(btnConfirm));
    };


  var validarInputVacios = function () {
    if ($("#pagoCash").val() == "") $("#pagoCash").val(0);
    if ($("#pagoZelle").val() == "") $("#pagoZelle").val(0);
    if ($("#pagoCheque").val() == "") $("#pagoCheque").val(0);
    if ($("#pagoCredito").val() == "") $("#pagoCredito").val(0);
    if ($("#pagoTransf").val() == "") $("#pagoTransf").val(0);
    if ($("#pagoWeb").val() == "") $("#pagoWeb").val(0);
    if ($("#PriceLb").val() == "") $("#PriceLb").val(0);
    if ($("#OtrosCostos").val() == "") $("#OtrosCostos").val(0);
    if ($("#ValorPagado").val() == "") $("#ValorPagado").val(0);
  };

  $(
    "#CantLb, #PriceLb, #OtrosCostos,#CantidadCombo,#check_credito,#pagoCash,#pagoZelle,#pagoCheque,#pagoCredito,#pagoTransf,#pagoWeb"
  ).on("change", validarInputVacios);

  calcMoney();

  $('[name="Delivery"]')
    .on("keyup", visibilityExpress)
    .on("change", visibilityExpress);

  function visibilityExpress() {
    var express = $("#container_express");
    if ($("#radioCO").is(":checked")) {
      var value = $('[name="Delivery"]').val();
      if (value > 0) {
        $(express).show();
      } else {
        $(express).hide();
        $("#express").prop("checked", false);
      }
    } else {
      $(express).show();
    }
    calcMoney();
  }

  //Mostrar y ocultar el credit card fee
  $('[name = "TipoPago"]').on("change", function () {
    var Id = $(this).val();
    tipoPago = $('option[value = "' + Id + '"]').html();
    if (tipoPago == "Crédito o Débito") {
      $("#contfee").show();
      calcMoney();
    } else {
      $("#contfee").hide();
      calcMoney();
    }
  });

  var changeTipo = function () {
    if (!norden) {
      setNoOrden();
      selectMayorista();
    }

    //******************
    // Para cargar el mayorista al cambiar el check
    //$("#PriceLb").val(pLb);
    //*******************
    if ($("#radioCA").is(":checked")) {
      //Para Cantidad
      $("#labelPrecio").html("Precio");
      $("#CantLbDiv").hide();
      $("#contNoOrden").hide();
      $('[name="contProductos"]').show(); //Productos y bolsas
      $("#contValoresAduanales").show();
      $("#ContCantLbDiv").show();
      $("#descripcionMayorista").hide();
      $("#contCantidadCombo").hide();
      $("#CantLb").val(cLb);
      cLb = 1;
      $("#OtrosCostos").val("0.90");
      $("#costCantidad").show();
      $("#Delivery").hide();
    } else if ($("#radioCO").is(":checked")) {
      // Para Combos
      $('[name = "contMayorista"]').hide();
      $('[name="contProductos"]').show(); //Productos y bolsas
      $("#labelPrecio").html("Precio");
      $("#contValoresAduanales").hide();
      $("#contNoOrden").hide();
      $("#CantLbDiv").hide();
      $("#descripcionMayorista").show();
      //$("#PriceLb").val(0);
      $("#CantLb").val(1);
      $('[name="ValorAduanal"]').val("").trigger("change");
      //Fila de añadir productos
      $("#newRow").hide();
      $("#bolsas").hide(); //Ocultar las bolsas
      $("#labelcrearbolsa").hide(); //boton de crear bolsa
      $("#OtrosCostos").val("1.00");
      $("#costCantidad").hide();
      $("#Delivery").show();
    } else if ($("#radioTI").is(":checked")) {
      //Para Tienda
      $("#labelPrecio").html("Precio");
      $("#ContCantLbDiv").hide();
      $("#contNoOrden").show();
      $('[name="contProductos"]').show(); //Productos y bolsas
      $("#contValoresAduanales").show();
      $("#CantLbDiv").show();
      $("#descripcionMayorista").hide();
      $("#contCantidadCombo").hide();
      $("#CantLb").val(cLb);
      cLb = 1;
      $("#OtrosCostos").val("0.90");
      $("#costCantidad").hide();
      $("#Delivery").hide();
    } else {
      $("#labelPrecio").html("Precio x Lb");
      $("#PriceLbDiv").css("display", "block");
      $("#ContCantLbDiv").css("display", "block");
      $("#contNoOrden").hide();
      $("#CantLbDiv").show();
      $('[name="contProductos"]').show(); //Productos y bolsas
      $("#contValoresAduanales").show();
      $("#CantLbDiv").show();
      $("#descripcionMayorista").hide();
      $("#contCantidadCombo").hide();
      $("#OtrosCostos").val("1.00");
      $("#costCantidad").hide();
      $("#Delivery").hide();
    }
    visibilityExpress();
    //$('#CantidadCombo').val(cCombo);
    cCombo = 1;
    if (!norden) {
      calcMoney();
    }
    norden = null;
  };
  changeTipo();
  $(
    "#radioMX, #radioPA, #radioAL, #radioME, #radioCA, #radioTI,#radioCO"
  ).click(changeTipo);

  $(
    "#CantLb, #PriceLb, #OtrosCostos,#CantidadCombo,#check_credito,#pagoCash,#pagoZelle,#pagoCheque,#pagoCredito,#pagoTransf,#pagoWeb"
  )
    .on("keyup", calcMoney)
    .on("change", calcMoney);

  $("#ValorPagado")
    .on("keyup", calcMoneyPayment)
    .on("change", calcMoneyPayment);

  $(".select2-placeholder-buscar").on("select2:select", function () {
    //Get the reference of the Table's TBODY element.
    var tBody = $("#tblProductos > tbody")[0];

    ////Add Row.
    var row = tBody.insertRow(-1);

    ////Add Cant cell.
    var cell = $(row.insertCell(-1));
    var celCant = $(
      "<input id='newCant' type='number' class='form-control' value='1' min='1'/>"
    );
    cell.append(celCant);

    cell = $(row.insertCell(-1));
    var celTipo = $("<input id='newTipo' class='form-control'/>");
    cell.append(celTipo);

    ////Add Remove cell.
    cell = $(row.insertCell(-1));
    var btnRemove = $("<input  />");
    btnRemove.attr("type", "button");
    btnRemove.attr("class", "btn btn-default");
    btnRemove.attr("onclick", "Remove");
    btnRemove.val("Eliminar");
    cell.append(btnRemove);
  });

  $(".select2-placeholder-selectProduct").on("select2:select", function () {
    var productId = $(this).val();

    //Busco el producto seleccionado
    var prodSelect;
    for (var i = 0; i < productos.length; i++) {
      if (productos[i].productId == productId) {
        prodSelect = productos[i];
        break;
      }
    }

    //Verifico que se haya añadido a la tabla
    var tabla = $("#tblProductos");
    var find = tabla.find('[data-id="' + productId + '"]');
    if (find.length == 0) {
      var desc = prodSelect.productName;
      $("#newDescrip").val(desc);

      $(".select2-placeholder-selectProduct").val("").trigger("change");

        addProductBodegaToTable(
            1,
            prodSelect.productName,
            prodSelect.productId,
            prodSelect.quantity,
            prodSelect.wholesalerName,
            prodSelect.wholesalerId
        );

      $("#newCant").val("1");
      $("#newDescrip").val("");

      updatePrecio();
    } else {
      toastr.warning(
        "El producto " + prodSelect.productName + " ya se añadió a la tabla"
      );
      $(".select2-placeholder-selectProduct").val("").trigger("change");
    }

    /*  $.ajax({
              type: "POST",
              url: "/ProductoBodega/getProducto",
              data: {
                  id: productId
              },
              async: false,
              success: function (data) {
                  console.log(data);
                  var desc = data.nombre;
  
                  $(".select2-placeholder-selectProduct").val("").trigger("change");
  
                  addProductToTable($("#newCant").val(), $("#newDescrip").val());
  
                  $("#newCant").val("1");
                  $("#newDescrip").val("");
              }
          });*/
  });

  $("#newCant").on("keypress", function (e) {
    if (e.which == 13) {
      $("#newDescrip").focus();
    }
  });

  $("#checkpago").on("click", function () {
    if ($("#checkpago").is(" :checked")) {
      $("#untipopago").attr("hidden", "hidden");
      $(".multipopago").removeAttr("hidden");
      $("#contfee").hide();
    } else {
      $(".multipopago").attr("hidden", "hidden");
      $("#untipopago").removeAttr("hidden");
    }
    calcMoney();
  });

  $("#check_credito").on("click", function () {
    $("#pagoCash").val(0);
    $("#pagoZelle").val(0);
    $("#pagoCheque").val(0);
    $("#pagoCredito").val(0);
    $("#pagoTransf").val(0);
    $("#pagoWeb").val(0);
  });

  /**************AuthCard************************************/
  $('[name="TipoPago"]').on("change", function () {
    var id = $(this).val();
    var value = $('option[value = "' + id + '"]').html();
    if (value == "Crédito o Débito") {
      $("#AddAuthorization").show();
    } else {
      $("#AddAuthorization").hide();
    }
  });

  $("#ConfirmAuth").click(function () {
    var error = false;
    if ($('[name="AuthTypeCard"]').val() == "") {
      error = true;
    }

    if ($('[name="AuthCardCreditEnding"]').val() == "") {
      error = true;
    }

    if ($('[name="AuthExpDate"]').val() == "") {
      error = true;
    }

    if ($('[name="AuthCCV"]').val() == "") {
      error = true;
    }

    if ($('[name="AuthaddressOfSend"]').val() == "") {
      error = true;
    }

    if ($('[name="AuthOwnerAddressDiferent"]').val() == "") {
      error = true;
    }

    if ($('[name="Authemail"]').val() == "") {
      error = true;
    }

    if ($('[name="Authphone"]').val() == "") {
      error = true;
    }

    if ($('[name="AuthConvCharge"]').val() == "") {
      error = true;
    }

    if ($('[name="TotalCharge"]').val() == "") {
      error = true;
    }

    if ($('[name="AuthSaleAmount"]').val() == "") {
      error = true;
    }

    $("#AuthorizationCard").modal("hide");
    $("body").removeClass("modal-open");
    $(".modal-backdrop").remove();
    if (!error) {
      $("#IcoAuthorizationAdd").attr("class", "fa fa-check-circle");
      $("#AddAuthorization").attr("class", "btn mr-1 mb-1 btn-success");
    } else {
      $("#IcoAuthorizationAdd").attr("class", "fa fa-plus-circle");
      $("#AddAuthorization").attr("class", "btn mr-1 mb-1 btn-secondary");
    }
  });

  $(document).ready(function () {
    $(".select2-container--default").attr("style", "width: 100%;");
  });

  // Para cargar el valor del precio por libra

  function getprecioxlibra() {
    var idcontacto = $('[name = "selectContact"]').val();
    //var idmayorista = $('#selectMayorista').val();
    var idmayorista = null;
    $.ajax({
      type: "POST",
      url: "/OrderNew/getPrecioxLibra",
      async: false,
      data: {
        idmayorista: idmayorista,
        idContacto: idcontacto,
      },
      beforeSend: function () {
        $.blockUI({
          message: '<div class="ft-refresh-cw icon-spin font-medium-2"></div>',
          timeout: 60000,
          overlayCSS: {
            backgroundColor: "#FFF",
            opacity: 0.8,
            cursor: "wait",
          },
          css: {
            border: 0,
            padding: 0,
            backgroundColor: "transparent",
          },
        });
      },
      success: function (data) {
        $('[name = "PriceLb"]').val(data);
        calcMoney();
      },
      failure: function (response) {
        showErrorMessage("ERROR", response.responseText);
      },
      error: function (response) {
        showErrorMessage("ERROR", response.responseText);
      },
    });
    $.unblockUI();
  }

  var isSend = false; //Para controlar que se envie una unica peticion

  var EditOrder = async function () {
    //Valido la orden
    if (!isSend && validateCreateOrder()) {
      isSend = true;

      //tomando las bolsas
      var listProductBolsa = new Array();
      var listCodigoBolsas = new Array();

      //tomando los productos
      var listProduct = new Array();

      var tBodys = $("#tblProductos > TBODY");
      var index = 0;
      tBodys.each(function () {
        var ttBody = $(this)[0];
        for (var i = 0; i < ttBody.rows.length; i++) {
          listProduct[index] = new Array();
          var fila = ttBody.rows[i];
          var packageItemId = $(fila).attr("shippingItem-id");

          for (var j = 0; j < 2; j++) {
            listProduct[index][j] = fila.children[j]
              ? $(fila.children[j]).html()
              : "";
          }

          //Verifico si es un producto de la bodega y le añado la descripción
          var id = $(fila).attr("data-id");
            if (id != null) {
                //Id Producto
                listProduct[index][3] = id;
                //Busco el producto
                for (var k = 0; k < productos.length; k++) {
                    if (productos[k].productId == id) {
                        //Descripción
                        listProduct[index][2] = productos[k].description;           
                        break;
                    }
                }
            }
            else {
                listProduct[index][2] = "";
                listProduct[index][3] = "";
            }

            listProduct[index][4] = packageItemId;

          index += 1;
        }
      });


      var valorespagado = [];
      var tipopagos = [];
      var notas = [];
      if (blockRight == "False") {
        if ($("#checkpago").is(":checked")) {
          valorespagado = [
            $("#pagoCash").val().replace(",", "."),
            $("#pagoZelle").val().replace(",", "."),
            $("#pagoCheque").val().replace(",", "."),
            $("#pagoCredito").val().replace(",", "."),
            $("#pagoTransf").val().replace(",", "."),
            $("#pagoWeb").val().replace(",", "."),
          ];
          notas = [
            "",
            $("#NotaZelle").val(),
            $("#NotaCheque").val(),
            "",
            $("#NotaTransf").val(),
            "",
          ];
          tipopagos = [
            "Cash",
            "Zelle",
            "Cheque",
            "Crédito o Débito",
            "Transferencia Bancaria",
            "Web",
          ];
        } else {
          valorespagado = [$("#ValorPagado").val().replace(",", ".")];
          tipopagos = [$(".hide-search-pago").val()];
          notas = [$("#NotaPago").val()];
        }
      }

      var pagoCrDeb = parseFloat($("#pagoCredito").val());
      var total = parseFloat($("#precioTotalValue").html());

      const attachments = document.getElementById('attachment').files;
      var attachment = null;
      if (attachments.length > 0)
        attachment = await getBase64Async(attachments[0]);

      var datosOrden = [
        $("#OrderId").val(), //0
        $("#no_orden").html(), //1
        $(".select2-placeholder-selectClient").val(), //2
        $(".select2-placeholder-selectContact").val(), //3
        tipopagos, //4
        listProduct, //5
        //$('#txtNameVA').val(), //6
        null, //6
        $("#CantLb").val().replace(",", "."), //7
        $("#PriceLb").val().replace(",", "."), //8
        $("#OtrosCostos").val().replace(",", "."), //9
        valorespagado, //10
        "0.00", //11
        total.toFixed(2), //12
        $("#balanceValue").html().replace(",", "."), //13
        $("#orderNote").val(), //14
        listCodigoBolsas, //15
        listProductBolsa, //16
        $("#AuthTypeCard").val(), //17
        $("#AuthCardCreditEnding").val(), //18
        $("#AuthExpDate").val(), //19
        $("#AuthCCV").val(), //20
        $("#AuthaddressOfSend").val(), //21
        $("#AuthOwnerAddressDiferent").val(), //22
        $("#Authemail").val(), //23
        $("#Authphone").val(), //24
        $("#AuthSaleAmount").val().replace(",", "."), //25
        $("#AuthConvCharge").val().replace(",", "."), //26
        $("#TotalCharge").val().replace(",", "."), //27
        $("#NoOrden").val(), //28
        $("#selectMayorista").val(), //29
        null, //30 para cuando sea un combo
        notas, //31 Cuando es tipo de pago Zell, Cheque o Transferencia Bancaria
        $("#express").is(":checked"), //32 Marcar como Express
        null, // 33 para los combos
        $("#check_credito").is(":checked")
          ? $("#credito").html().replace(",", ".")
          : 0, // 34
        $("#costoMayorista").val(), //35
        blockRight, //36
        $("[name='Delivery']").val(), //37
        '', //38
        '', //39
        attachment, //40
        0, //Cargos Adicionales - Paquete
        $("#ProductsShipping").val(), //42
          new Array(), //43,
        '' //44 Minorista 
      ];
      //Hago la extraccion de los productos de la bodega

      guardarEnvio(datosOrden);
    }

    return true;
  };

  function guardarEnvio(datosOrden) {
    $.ajax({
      type: "POST",
      url: "/OrderNew/EditOrder",
      data: JSON.stringify(datosOrden),
      dataType: "json",
      contentType: "application/json",
      async: true,
        beforeSend: function () {
            $.blockUI();
        },
      success: function (data) {
        if (data.status == "success") {
          window.location =
            "/Orders/Details/" +
            data.orderId +
            "?msg=success&orderNumber=" +
            $("#no_orden").html();
        } else {
          removeBags();
          toastr.error("No se ha podido crear el trámite", "ERROR");
        }
          $.unblockUI();
          isSend = false;
      },
      failure: function (response) {
        showErrorMessage("FAILURE", "No se ha podido crear el trámite");
          $.unblockUI();
          isSend = false;
      },
      error: function (response) {
        showErrorMessage("ERROR", "No se ha podido crear el trámite");
          $.unblockUI();
          isSend = false;
      },
    });
  }

  var validateCreateOrder = function () {
    if ($("#CantLb").val() == "") {
      showWarningMessage("Atención", "El campo Peso(Lb) no puede estar vacío.");
      return false;
    } else if ($("#PriceLb").val() == "" && !$("#radioCA").is(":checked")) {
      showWarningMessage("Atención", "El campo Precio no puede estar vacío.");
      return false;
    } else if ($("#OtrosCostos").val() == "") {
      showWarningMessage(
        "Atención",
        "El campo Otros Cargos no puede estar vacío."
      );
      return false;
    } else if ($("#ValorPagado").val() == "") {
      showWarningMessage(
        "Atención",
        "El campo Importe Pagado no puede estar vacío."
      );
      return false;
    } else if (parseFloat($("#balanceValue").html()) < 0) {
      showWarningMessage(
        "Atención",
        "El Importe Pagado no puede ser superior al Precio Total."
      );
      return false;
    } else if ($("#CantLb").val() <= 0) {
      showWarningMessage("Atención", "El campo Peso(Lb) debe ser mayor que 0.");
      return false;
    } else if ($("#PriceLb").val() <= 0 && !$("#radioCA").is(":checked")) {
      showWarningMessage("Atención", "El campo Precio debe ser mayor que 0.");
      return false;
    } else if ($("#NoOrden").val() == "" && $("#radioTI").is(":checked")) {
      showWarningMessage(
        "Atención",
        "El campo Número de Orden no puede estar vacío."
      );
      return false;
    } else if ($("#OtrosCostos").val() < 0) {
      showWarningMessage(
        "Atención",
        "El campo Otros Cargos debe ser mayor o igual que 0."
      );
      return false;
    } else if ($("#ValorPagado").val() < 0) {
      showWarningMessage(
        "Atención",
        "El campo Importe Pagado debe ser mayor o igual que 0."
      );
      return false;
    }
    if (!$("#no_orden").html().includes("CO")) embolsar();

    return true;
  };


    function cargarproductos(tipoEnvio) {
        productos = new Array();
    var esCombo = false;
    if (tipoEnvio == "CO") {
      esCombo = true;
    }
    $.ajax({
      type: "GET",
      url: "/BodegaProducto/getProduct",
      data: {
        category: "Combos",
      },
      async: false,
      success: function (data) {
        productos = data; //Guardo los productos en una variable global

        $('[name = "selectProduct"]').empty();
        $('[name = "selectProduct"]').append(
          new Option("Seleccione los productos", "", true)
        );
        if (data.length != 0) {
            for (var i = 0; i < data.length; i++) {
                if (data[i].wholesalerId != wholesalerId)
                    continue;

            //var categorySelected = $('[name = "orderType"][checked]').val();
            if (idMayoristabyTransferencia != "") {
              //Si es por transferencia no muestro el mayorista
              var x = new Option(data[i].productName, data[i].productId);
              x.title = data[i].description;
              $('[name = "selectProduct"]').append(x);
            } else {
              var x = new Option(
                data[i].productName + " - " + data[i].wholesalerName,
                data[i].productId
              );
              x.title = data[i].description;
              $('[name = "selectProduct"]').append(x);
            }
          }
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
  cargarproductos("MX");

  function removeBags() {
    var bolsas = $(".btnRemoveBolsa");
    for (var i = 0; i < bolsas.length; i++) {
      var btnremove = bolsas[i];
      var html = $($(btnremove).parent().parent().parent());
      var bolsa = $(btnremove).parent().parent();
      //Si hay productos de la bodega modifico las cantidades
      var table = $(bolsa).find("table");
      var trs = table.find("tbody")[0].children;
      for (var i = 0; i < trs.length; i++) {
        var tr = trs[i];
        var idprod = $(tr).attr("data-id");
        if (idprod != null) {
          var cant = parseFloat(tr.firstChild.innerHTML);
          //Busco el producto y le sumo la cantidad
          for (var j = 0; j < productos.length; j++) {
            if (productos[j].productId == idprod) {
              //Si la cantidad del producto =  0 lo agrego al select ya que al ser 0 se habia eliminado del select
              if (parseFloat(productos[j].quantity) == 0) {
                //SI es combo se le añade el mayorista al nombre
                if ($("#radioCO").is(":checked")) {
                  var x = new Option(
                    productos[j].productName +
                    " - " +
                    productos[j].wholesalerName,
                    productos[j].productId,
                    false,
                    false
                  );
                  x.title = productos[j].description;
                  $('[name = "selectProduct"]').append(x);
                } else {
                  var x = new Option(
                    productos[j].productName,
                    productos[j].productId,
                    false,
                    false
                  );
                  x.title = productos[j].description;
                  $('[name = "selectProduct"]').append(x);
                }
              }
              var precioproducto = parseFloat(productos[j].salePrice);
              preciosBolsas -= precioproducto * cant;
              updatePrecio();
              productos[j].quantity += cant;
              break;
            }
          }
        }
      }

      $(bolsa).remove();
      if (html.find("div").length === 0) {
        $("#noBolsas").show();
      }
    }
  }

  function getBase64Async(file) {
    return new Promise((resolve, reject) => {
      let reader = new FileReader();

      reader.onload = () => {
        resolve(reader.result);
      };

      reader.onerror = reject;

      reader.readAsDataURL(file);
    })
    }

    async function LoadPackageItems() {
        //Getproducts 
        var shippingItems = await $.get("/OrderNew/GetProductsCombo?orderId=" + orderId);
        if (shippingItems) {
            for (var i = 0; i < shippingItems.length; i++) {
                var item = shippingItems[i];
                addProductBodegaToTable(item.qty, item.description, item.id, item.quantity, item.wholesalerName, item.wholesalerId, item.packageItemId);

            }
        }
    }

    LoadPackageItems();
});
