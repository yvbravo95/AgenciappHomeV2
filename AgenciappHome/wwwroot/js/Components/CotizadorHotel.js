"use strict";

const e = React.createElement;

const initHabitacion = {
  tipo: "",
  cantidadAdultos: 0,
  precioAdulto: 0,
  cantidadInfantes: 0,
  precioInfantes: 0,
  cantidadMenores: 0,
  precioMenores: [],
  precioTotal: 0,
  comentarios: "",
};

const initHotel = {
  name: "",
  ubicacion: "",
  precioAvionAdulto: 0,
  precioAvionMenor: 0,
  precioAvionInfante: 0,
  precioAvionTotal: 0,
  precioAuto: 0,
  precioTotal: 0,
  depositoInicial: 0,
  habitaciones: [{ ...initHabitacion }],
  link: "",
};

const initHotelList = hoteles || {
  hotels: [{ ...initHotel }],
  nombreAgente: "",
  contacto: "",
};

class HotelList extends React.Component {
  constructor(props) {
    super(props);
    this.state = { ...initHotelList };
  }

  componentDidMount(prevProps) {
    this.vaildate();
  }

  componentDidUpdate(prevProps) {
    this.vaildate();
  }

  addHotel = () => {
    var x = [...this.state.hotels, { ...initHotel }];
    this.setState({ ...this.state, hotels: x });
  };

  remHotel = (i) => {
    var x = [...this.state.hotels];
    x.splice(i, 1);
    this.setState({ ...this.state, hotels: x });
  };

  handleChange = (i, value) => {
    var x = [...this.state.hotels];
    x[i] = value;
    this.setState({ ...this.state, hotels: x });
  };

  vaildate = () => {
    var e = "";
    this.state.hotels.map((hotel) => {
      hotel.habitaciones.map((habitacion) => {
        if (!habitacion.tipo) {
          e = "El campo tipo es requerido";
        } else if (
          !(
            habitacion.cantidadAdultos ||
            habitacion.cantidadInfantes ||
            habitacion.cantidadMenores
          )
        ) {
          e = "Debe haber una persona en la habitación";
        } else if (
          (habitacion.cantidadAdultos && !habitacion.precioAdulto) ||
          (habitacion.cantidadInfantes && !habitacion.precioInfantes) ||
          (habitacion.cantidadMenores &&
            habitacion.precioMenores.some((x) => !x))
        ) {
          e = "El campo precio es requrido";
        } else if (!hotel.name) {
          e = "El nombre de hotel es requerido";
        }
      });
    });
    error = e;
  };

  render() {
    return (
      <div>
        <div className="row">
          <div className="col-md-4">
            <label for="nombreAgente">
              <b>Nombre del Agente</b>
            </label>
            <div className="form-group ">
              <input
                className="form-control"
                id="nombreAgente"
                placeholder="Nombre del Agente"
                name="nombreAgente"
                type="text"
                value={this.state.nombreAgente}
                onChange={(e) => {
                  const value = e.target.value;
                  this.setState({ ...this.state, nombreAgente: value });
                }}
              />
            </div>
          </div>
          <div className="col-md-4">
            <label for="contacto">
              <b>Contacto</b>
            </label>
            <div className="form-group ">
              <input
                className="form-control"
                id="conacto"
                placeholder="Contacto"
                name="contacto"
                type="text"
                value={this.state.contacto}
                onChange={(e) => {
                  const value = e.target.value;
                  this.setState({ ...this.state, contacto: value });
                }}
              />
            </div>
          </div>
        </div>
        <div
          style={{
            textAlign: "right",
            marginBottom: 20,
          }}
        >
          <a
            className="btn btn-primary btn-sm text-white"
            id="createHotel"
            onClick={() => this.addHotel()}
          >
            Agregar Hotel
          </a>
        </div>
        {this.state.hotels.map((x, i) => (
          <div>
            <Hotel
              count={i}
              data={x}
              onChange={(value) => {
                this.handleChange(i, value);
              }}
              remHotel={() => {
                this.remHotel(i);
              }}
            />
          </div>
        ))}
      </div>
    );
  }
}

class Hotel extends React.Component {
  constructor(props) {
    super(props);
  }

  onChange = (e) => {
    var value = e.target.value;
    var [id, count] = e.target.id.split("_");
    var data = { ...this.props.data };
    data[id] = value;

    var pAvion = data.precioAvionTotal ? parseFloat(data.precioAvionTotal) : 0;
    var pAuto = data.precioAuto ? parseFloat(data.precioAuto) : 0;

    var precioTotal =
      pAvion + pAuto + data.habitaciones.reduce((p, x) => p + x.precioTotal, 0);

    data.precioTotal = precioTotal;

    this.props.onChange(data);
  };

  addHabitacion = () => {
    var h = [
      ...this.props.data.habitaciones,
      {
        ...initHabitacion,
      },
    ];
    this.props.onChange({ ...this.props.data, habitaciones: h });
  };

  remHabitacion = (i) => {
    var h = [...this.props.data.habitaciones];
    h.splice(i, 1);

    var data = this.props.data;
    var pAvion = data.precioAvionTotal ? parseFloat(data.precioAvionTotal) : 0;
    var pAuto = data.precioAuto ? parseFloat(data.precioAuto) : 0;

    var precioTotal = pAvion + pAuto + h.reduce((p, x) => p + x.precioTotal, 0);
    this.props.onChange({ ...this.props.data, habitaciones: h, precioTotal });
  };

  onChangeHabitacion(i, value) {
    var h = [...this.props.data.habitaciones];
    h[i] = value;
    var data = this.props.data;
    var pAvion = data.precioAvionTotal ? parseFloat(data.precioAvionTotal) : 0;
    var pAuto = data.precioAuto ? parseFloat(data.precioAuto) : 0;

    var precioTotal = pAvion + pAuto + h.reduce((p, x) => p + x.precioTotal, 0);

    this.props.onChange({ ...data, habitaciones: h, precioTotal });
  }

  render() {
    return (
      <div style={{ marginBotton: 15 }}>
        <h4>
          {" "}
          Hotel {this.props.count + 1}{" "}
          {this.props.count !== 0 && (
            <span
              style={{
                marginLeft: 20,
              }}
            >
              <a
                className=""
                id="createHotel"
                onClick={() => this.props.remHotel()}
              >
                <i className="fa fa-trash danger"></i>
              </a>
            </span>
          )}
        </h4>

        <div className="row">
          <div className="col-md-4">
            <label for={`name_${this.props.count}`}>
              <b>Nombre</b>
            </label>
            <div className="form-group ">
              <input
                className="form-control"
                id={`name_${this.props.count}`}
                placeholder="Nombre"
                name={`hotels[${this.props.count}].name`}
                type="text"
                value={this.props.data.name}
                onChange={(e) => this.onChange(e)}
              />
            </div>
          </div>
          <div className="col-md-4">
            <label for={`ubicacion_${this.props.count}`}>
              <b>Ubicación</b>
            </label>
            <div className="form-group ">
              <input
                className="form-control"
                id={`ubicacion_${this.props.count}`}
                placeholder="Ubicación"
                name={`hotels[${this.props.count}].ubicacion`}
                type="text"
                value={this.props.data.ubicacion}
                onChange={(e) => this.onChange(e)}
              />
            </div>
          </div>
          <div className="col-md-4">
            <label for={`link_${this.props.count}`}>
              <b>Link</b>
            </label>
            <div className="form-group ">
              <input
                className="form-control"
                id={`link_${this.props.count}`}
                placeholder="Link"
                name={`hotels[${this.props.count}].link`}
                type="text"
                value={this.props.data.link}
                onChange={(e) => this.onChange(e)}
              />
            </div>
          </div>
        </div>
        <h5>Habitaciones</h5>
        <div
          style={{
            textAlign: "right",
            marginBottom: 20,
          }}
        >
          <a
            className="btn btn-primary btn-sm text-white"
            onClick={() => this.addHabitacion()}
          >
            Agregar Habitación
          </a>
        </div>
        {this.props.data.habitaciones.map((x, i) => (
          <Habitacion
            noHotel={this.props.count}
            noHab={i}
            onChange={(value) => this.onChangeHabitacion(i, value)}
            data={x}
            remHabitacion={() => this.remHabitacion(i)}
          />
        ))}
        <div className="row">
          <div className="col-md-3">
            <label for={`precioAvionAdulto_${this.props.count}`}>
              <b>Precio Avión Adulto</b>
            </label>
            <div className="form-group ">
              <input
                className="form-control"
                id={`precioAvionAdulto_${this.props.count}`}
                placeholder="Precio Avión Adulto"
                name={`hotels[${this.props.count}].precioAvionAdulto`}
                type="number"
                value={this.props.data.precioAvionAdulto}
                onChange={(e) => this.onChange(e)}
              />
            </div>
          </div>
          <div className="col-md-3">
            <label for={`precioAvionInfante_${this.props.count}`}>
              <b>Precio Avión Infante</b>
            </label>
            <div className="form-group ">
              <input
                className="form-control"
                id={`precioAvionInfante_${this.props.count}`}
                placeholder="Precio Avión Infante"
                name={`hotels[${this.props.count}].precioAvionInfante`}
                type="number"
                value={this.props.data.precioAvionInfante}
                onChange={(e) => this.onChange(e)}
              />
            </div>
          </div>
          <div className="col-md-3">
            <label for={`precioAvionTotal_${this.props.count}`}>
              <b>Precio Avión Total</b>
            </label>
            <div className="form-group ">
              <input
                className="form-control"
                id={`precioAvionTotal_${this.props.count}`}
                placeholder="Precio Avión Total"
                name={`hotels[${this.props.count}].precioAvionTotal`}
                type="number"
                value={this.props.data.precioAvionTotal}
                onChange={(e) => this.onChange(e)}
              />
            </div>
          </div>
          <div className="col-md-3">
            <label for={`precioAuto_${this.props.count}`}>
              <b>Precio Auto</b>
            </label>
            <div className="form-group ">
              <input
                className="form-control"
                id={`precioAuto_${this.props.count}`}
                placeholder="Precio Auto"
                name={`hotels[${this.props.count}].precioAuto`}
                type="number"
                value={this.props.data.precioAuto}
                onChange={(e) => this.onChange(e)}
              />
            </div>
          </div>
          <div className="col-md-3">
            <label for={`precioTotal_${this.props.count}`}>
              <b>Precio Total</b>
            </label>
            <div className="form-group ">
              <input
                readonly
                className="form-control"
                id={`precioTotal_${this.props.count}`}
                placeholder="Precio Total"
                name={`hotels[${this.props.count}].precioTotal`}
                type="number"
                value={this.props.data.precioTotal}
                onChange={(e) => this.onChange(e)}
              />
            </div>
          </div>
          <div className="col-md-3">
            <label for={`depositoInicial_${this.props.count}`}>
              <b>Depósito Inicial</b>
            </label>
            <div className="form-group ">
              <input
                className="form-control"
                id={`depositoInicial_${this.props.count}`}
                placeholder="Depósito Inicial"
                name={`hotels[${this.props.count}].depositoInicial`}
                type="number"
                value={this.props.data.depositoInicial}
                onChange={(e) => this.onChange(e)}
              />
            </div>
          </div>
        </div>
        <hr style={{ borderTop: "3px solid #bbb" }} />
      </div>
    );
  }
}

class Habitacion extends React.Component {
  constructor(props) {
    super(props);
  }

  onChange(e) {
    var value = e.target.value;
    var [id, count] = e.target.id.split("_");
    var data = { ...this.props.data };
    data[id] = value;

    var pAdulto =
      data.cantidadAdultos && data.precioAdulto
        ? data.cantidadAdultos * data.precioAdulto
        : 0;

    var pInfante =
      data.cantidadInfantes && data.precioInfantes
        ? data.cantidadInfantes * data.precioInfantes
        : 0;

    var precioTotal =
      pAdulto +
      pInfante +
      data.precioMenores.reduce((p, x) => (x ? parseFloat(x) + p : p), 0);

    data.precioTotal = precioTotal;

    this.props.onChange(data);
  }

  onChangePrecioMenor(i, value) {
    var data = { ...this.props.data };
    var precioMenores = [...data.precioMenores];
    precioMenores[i] = value;
    data.precioMenores = precioMenores;

    var pAdulto =
      data.cantidadAdultos && data.precioAdulto
        ? data.cantidadAdultos * data.precioAdulto
        : 0;

    var pInfante =
      data.cantidadInfantes && data.precioInfantes
        ? data.cantidadInfantes * data.precioInfantes
        : 0;

    var precioTotal =
      pAdulto +
      pInfante +
      data.precioMenores.reduce((p, x) => (x ? parseFloat(x) + p : p), 0);

    data.precioTotal = precioTotal;

    this.props.onChange(data);
  }

  onChangeCantMenores(e) {
    var value = e.target.value;

    var precioMenores;
    var i = value - this.props.data.precioMenores.length;

    if (value == 0) {
      precioMenores = [];
    } else if (i < 0) {
      precioMenores = this.props.data.precioMenores.slice(0, value);
    } else {
      precioMenores = [...this.props.data.precioMenores, ...Array(i).fill(0)];
    }

    var data = { ...this.props.data, precioMenores, cantidadMenores: value };

    var pAdulto =
      data.cantidadAdultos && data.precioAdulto
        ? data.cantidadAdultos * data.precioAdulto
        : 0;

    var pInfante =
      data.cantidadInfantes && data.precioInfantes
        ? data.cantidadInfantes * data.precioInfantes
        : 0;

    var precioTotal =
      pAdulto +
      pInfante +
      data.precioMenores.reduce((p, x) => (x ? parseFloat(x) + p : p), 0);

    data.precioTotal = precioTotal;
    this.props.onChange(data);
  }

  render() {
    var noHotel = this.props.noHotel;
    var noHab = this.props.noHab;

    return (
      <div>
        <div>
          Habitación {noHab + 1}
          {noHab !== 0 && (
            <span
              style={{
                marginLeft: 20,
              }}
            >
              <a className="" onClick={() => this.props.remHabitacion()}>
                <i className="fa fa-trash danger"></i>
              </a>
            </span>
          )}
        </div>
        <div className="row">
          <div className="col-md-3">
            <label for={`tipo_${noHotel}_${noHab}`}>
              <b>Tipo</b>
            </label>
            <div className="form-group ">
              <input
                className="form-control"
                id={`tipo_${noHotel}_${noHab}`}
                placeholder="Tipo"
                name={`hotels[${noHotel}].habitaciones[${noHab}].tipo`}
                value={this.props.data.tipo}
                onChange={(e) => this.onChange(e)}
              />
            </div>
          </div>
          <div className="col-md-3">
            <label for={`cantidadAdultos_${noHotel}_${noHab}`}>
              <b>Cantidad Adultos</b>
            </label>
            <div className="form-group ">
              <input
                className="form-control"
                id={`cantidadAdultos_${noHotel}_${noHab}`}
                placeholder="Cantidad Adultos"
                name={`hotels[${noHotel}].habitaciones[${noHab}].cantidadAdultos`}
                type="number"
                min="0"
                value={this.props.data.cantidadAdultos}
                onChange={(e) => this.onChange(e)}
              />
            </div>
          </div>
          <div className="col-md-3">
            <label for={`cantidadMenores_${noHotel}_${noHab}`}>
              <b>Cantidad Niños</b>
            </label>
            <div className="form-group ">
              <input
                className="form-control"
                min="0"
                id={`cantidadMenores_${noHotel}_${noHab}`}
                placeholder="Cantidad Adultos"
                name={`hotels[${noHotel}].habitaciones[${noHab}].cantidadMenores`}
                type="number"
                value={this.props.data.cantidadMenores}
                onChange={(e) => this.onChangeCantMenores(e)}
              />
            </div>
          </div>
          <div className="col-md-3">
            <label for={`cantidadInfantes_${noHotel}_${noHab}`}>
              <b>Cantidad Infantes</b>
            </label>
            <div className="form-group ">
              <input
                className="form-control"
                min="0"
                id={`cantidadInfantes_${noHotel}_${noHab}`}
                placeholder="Cantidad Infantes"
                name={`hotels[${noHotel}].habitaciones[${noHab}].cantidadInfantes`}
                type="number"
                value={this.props.data.cantidadInfantes}
                onChange={(e) => this.onChange(e)}
              />
            </div>
          </div>
        </div>
        <div className="row">
          <div className="col-md-3">
            <label for={`precioAdulto_${noHotel}_${noHab}`}>
              <b>Precio Adultos</b>
            </label>
            <div className="form-group ">
              <input
                className="form-control"
                min="0"
                step="0.01"
                id={`precioAdulto_${noHotel}_${noHab}`}
                placeholder="Precio Adultos"
                name={`hotels[${noHotel}].habitaciones[${noHab}].precioAdulto`}
                type="number"
                value={this.props.data.precioAdulto}
                onChange={(e) => this.onChange(e)}
              />
            </div>
          </div>
          {this.props.data.precioMenores &&
            this.props.data.precioMenores.map((p, i) => (
              <div className="col-md-3">
                <label for={`precioMenores_${noHotel}_${noHab}_${i}`}>
                  <b>Precio Niño {i + 1}</b>
                </label>
                <div className="form-group ">
                  <input
                    className="form-control"
                    min="0"
                    step="0.01"
                    id={`precioMenores_${noHotel}_${noHab}_${i}`}
                    placeholder={`Precio Niño ${i + 1}`}
                    name={`hotels[${noHotel}].habitaciones[${noHab}].precioMenores[${i}]`}
                    type="number"
                    value={p}
                    onChange={(e) =>
                      this.onChangePrecioMenor(i, e.target.value)
                    }
                  />
                </div>
              </div>
            ))}
          <div className="col-md-3">
            <label for={`precioInfantes_${noHotel}_${noHab}`}>
              <b>Precio Infantes</b>
            </label>
            <div className="form-group ">
              <input
                className="form-control"
                min="0"
                step="0.01"
                id={`precioInfantes_${noHotel}_${noHab}`}
                placeholder="Precio Infantes"
                name={`hotels[${noHotel}].habitaciones[${noHab}].precioInfantes`}
                type="number"
                value={this.props.data.precioInfantes}
                onChange={(e) => this.onChange(e)}
              />
            </div>
          </div>
          <div className="col-md-3">
            <label for={`precioTotal_${noHotel}_${noHab}`}>
              <b>Precio Total</b>
            </label>
            <div className="form-group ">
              <input
                className="form-control"
                readonly
                min="0"
                step="0.01"
                id={`precioTotal_${noHotel}_${noHab}`}
                placeholder="Precio Total"
                name={`hotels[${noHotel}].habitaciones[${noHab}].precioTotal`}
                type="number"
                value={this.props.data.precioTotal}
                onChange={(e) => this.onChange(e)}
              />
            </div>
          </div>
        </div>
        <div class="row">
          <div class="col-md-12">
            <fieldset class="form-group">
              <label for={`comentarios_${noHotel}_${noHab}`}>
                <b>Comentarios</b>
              </label>
              <textarea
                class="form-control"
                id={`comentarios_${noHotel}_${noHab}`}
                name={`hotels[${noHotel}].habitaciones[${noHab}].comentarios`}
                value={this.props.data.comentarios}
                onChange={(e) => this.onChange(e)}
                rows="2"
                placeholder="Comentarios"
              ></textarea>
            </fieldset>
          </div>
        </div>
      </div>
    );
  }
}

const domContainer = document.querySelector("#hotel_list_container");
ReactDOM.render(e(HotelList), domContainer);
