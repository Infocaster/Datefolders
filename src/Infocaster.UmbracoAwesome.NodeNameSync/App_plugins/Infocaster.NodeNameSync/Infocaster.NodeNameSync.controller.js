angular.module("umbraco").controller("Infocaster.NodeNameSync", function ($scope, editorState) {
    var vm = this;
    var nameChangedLast = true;
    vm.inSync = editorState.getCurrent().name === $scope.model.value;

    // From node name to model
    var fromNameToModel = function(newValue) {
        if (vm.inSync) {
            $scope.model.value = newValue;
        }
        nameChangedLast = true;
    };
    $scope.$watch(function () { return editorState.getCurrent().name; }, fromNameToModel);

    // From model to node name
    var fromModelToName = function(newValue) {
        if (vm.inSync) {
            editorState.getCurrent().name = newValue;
        }
        nameChangedLast = false;
    };
    $scope.$watch(function () { return $scope.model.value; }, fromModelToName);

    vm.toggleSync = function () {
        vm.inSync = !vm.inSync;
        if (nameChangedLast) {
            fromNameToModel(editorState.getCurrent().name);
        } else {
            fromModelToName($scope.model.value);
        }
    };
});