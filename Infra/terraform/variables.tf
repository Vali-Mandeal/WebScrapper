variable "location" {
  description = "Azure region for resources"
  type        = string
  default     = "West Europe"
}

variable "resource_group_name" {
  description = "Name of the resource group (created by bootstrap)"
  type        = string
  default     = "rg-webscrapper"
}

variable "key_vault_admin_object_id" {
  description = "Azure AD object ID of the admin user for Key Vault full access"
  type        = string
}

variable "master_image" {
  description = "Full GHCR image reference for the master function (e.g., ghcr.io/owner/webscrapper-master:sha)"
  type        = string
}

variable "slave_image" {
  description = "Full GHCR image reference for the slave function (e.g., ghcr.io/owner/webscrapper-slave:sha)"
  type        = string
}
