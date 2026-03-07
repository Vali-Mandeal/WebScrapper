output "master_container_app_name" {
  value = azapi_resource.function_master.name
}

output "slave_container_app_name" {
  value = azapi_resource.function_slave.name
}
