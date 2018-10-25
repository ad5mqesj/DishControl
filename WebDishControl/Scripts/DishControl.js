$(document).ready(function () {
    viewModel.Load();
});

class dishStatus
{
    constructor() {
        this.connected = ko.observable();
        this.state = ko.observable();
        this.tracking = ko.observable();
    }

    connectedColor() {
        if (this.connected())
            return 'Green';
        else
            return 'Red';
    }

    stateColor() {
        if (this.state() == 'Stopped')
            return 'Green';
        else if (this.state() == 'Unknown')
            return 'Red';
        else
            return 'Yellow';
    }

    trackingColor() {
        if (this.tracking())
            return 'Blue';
        return 'Transparent';
    }

}

var viewModel = {
    basePath: null,
    RA: ko.observable("0:0:0"),
    DEC: ko.observable("0:0:0"),
    Azimuth: ko.observable("0:0:0"),
    Elevation: ko.observable("0:0:0"),

    RACommand: ko.observable(),
    DECCommand: ko.observable(),
    AzCommand: ko.observable(),
    ElCommand: ko.observable(),
    status: null,

    timerId: -1,

    PushIfUnique: function (array, value, predicate) {
        // will unwrap (ie () ) if needed, do nothing if not
        var data = ko.utils.unwrapObservable(array);
        var clean = true;
        $.each(array(), function (index, avalue) {
            if (predicate(avalue)) {
                clean = false;
                return false;
            }
        });
        if (clean) {
            array.push(value);
        }
    },

    Load: function () {
        var self = viewModel;
        self.basePath = 'http://localhost:9000/DishControl/';
        self.status = new dishStatus();
        self.timerId = setInterval(self.timerCallback, 200);
        ko.applyBindings();
    },

    timerCallback: function () {
        var self = viewModel;
        $.get(self.basePath + 'api/Dish/GetPosition', function (data) {
            self.Azimuth(data.formattedAzimuth);
            self.Elevation(data.formattedElevation);
            self.RA(data.formattedRightAscension);
            self.DEC(data.formattedDeclination);
        });
        $.get(self.basePath + 'api/Dish/GetStatus', function (data) {
            self.status.connected(data.Connected);
            self.status.tracking(data.Tracking);
            self.status.state(data.State);
        });
    },

};