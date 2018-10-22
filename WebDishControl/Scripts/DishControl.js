$(document).ready(function () {
    viewModel.Load();
});

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

        self.timerId = setInterval(self.timerCallback, 200);
    },

    timerCallback: function () {
        var self = viewModel;
        $.get(self.basePath + 'api/Dish/GetPosition', function (data) {
            self.Azimuth(data.formattedAzimuth);
            self.Elevation(data.formattedElevation);
            self.RA(data.formattedRightAscension);
            self.DEC(data.formattedDeclination);
        });
    },

};