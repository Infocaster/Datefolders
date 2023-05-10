angular.module("umbraco").controller("Infocaster.NodeNameSync", function ($scope, editorState) {
    var vm = this;
    var nameChangedLast = true;
    $scope.editorState = editorState;
    $scope.editorStateCurrentCulture = $scope.editorState.current.variants.find(element => element.active);

    vm.inSync = $scope.editorStateCurrentCulture.name === $scope.model.value;
    $scope.syncField = {
        view: 'textbox',
        config: {},
        value: $scope.model.value
    };

    $scope.$watch(function () { return $scope.syncField.value; }, (value) => {
        $scope.fromModelToName(value);
    });

    $scope.$watch(function () {
        return $scope.editorState.current.variants.find(element => element.active).name;
    }, (value) => {
        $scope.fromNameToModel(value);
    });

    // From node name to model
    $scope.fromNameToModel = function (newValue) {

        if (vm.inSync) {
            $scope.syncField.value = newValue;
            $scope.updateModel();
        }
        nameChangedLast = true;
    };

    // From model to node name
    $scope.fromModelToName = function (newValue) {

        if (vm.inSync) {
            $scope.editorStateCurrentCulture.name = newValue;
        }

        nameChangedLast = false;
        $scope.updateModel();
    };

    vm.toggleSync = function () {
        vm.inSync = !vm.inSync;

        if (nameChangedLast) {
            $scope.fromNameToModel($scope.editorStateCurrentCulture.name);
        } else {
            $scope.fromModelToName($scope.model.value);
        }
    };

    $scope.updateModel = function () {
        $scope.model.value = $scope.syncField.value;
    };
});