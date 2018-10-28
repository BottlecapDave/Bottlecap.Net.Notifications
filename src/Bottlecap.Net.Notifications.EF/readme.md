# Bottlecap.Net.Notifications.EF

EntityFramework extension for **Bottlecap.Net.Notifications**.

## Setup

A ServiceCollection extension exists for adding the notification service and dependencies, including the EntityFramework implementation for storage and retrieval of notifications.

This is in the form of **AddNotificationServiceWithEF** in the **Bottlecap.Net.Notifications.EF** namespace.

You will need to provide an implementation of IDataContext which is your DbContext instance, where access to the notifications tables will exist.