# Warehouse module: Parking

With this module, you can fetch data from parking companies into your own data warehouse on Azure.
Right now, it only fetches data from EasyPark.

The module is build to use this [API from EasyPark](https://external-gw.easyparksystem.net/api/swagger-ui.html), and save data in your own data warehouse on Azure.

It's possible to add data from multiple EasyPark operators and multiple area-numbers.

You need this from EasyPark before you can use this module:
* A username
* Password
* Operator id
* Area number
It is normally municipalities or firms that are using EasyPark, that can get these information's.

## Installation

All modules can be installed and facilitated with ARM templates (Azure Resource Management): [Use ARM templates to setup and maintain this module](https://github.com/hillerod/Warehouse.Modules.Parking/blob/master/Deploy).

# License

[MIT License](https://github.com/Bygdrift/Warehouse.Modules.DaluxFMApi/blob/master/License.md)
