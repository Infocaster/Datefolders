import { ngNodeNameSync } from "./nodenamesync/directive";

const module = angular.module('umbraco');

module.directive(ngNodeNameSync.alias, ngNodeNameSync);