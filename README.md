# Bottlecap.Net.Notifications
Bottlecap.Net.Notifications is a component built to support sending notifications quickly. It's main features are
* Plugable transport mechanisms
* Ability to retry failed notifications
* Support for sending notifications straight away or via a background service.
* Ability to define notification once, but have it sent via many different mechanisms at the same time.

These are the main parts around Bottlecap.Net.Notifications

## INotificationTransporter
This represents a transport mechanism. This is where notifications will be sent via once it has been determined that the notification supports the transporter (see INotificationRecipientResolver). 

Its responsibility is to send the provided notification, potentially converting the notification into something that can be sent before hand. For example, an email based transporter might take the provided content and merge it into an email template associated with the notification.

Currently, there are implementations for the following services
* [SendGrid](./src/Bottlecap.Net.Notifications.Transporters.SendGrid/readme.md)

## INotificationRecipientResolver
Used to resolve the provided recipient into a recipient object the associated transporter can understand. For example, an email based transporter will use this to resolve the recipient into a collection of email addresses.

The shape of these will be dependent on which transporters you register.

## NotificationService
This is the main service for sending notifications. It provides the following methods.

### Schedule
Schedule the provided notification for all applicable transporters for the provided recipient. 

The purpose of this is that the scheduled notification will be executed at a later date by an external service.

### Execute

Attempts to send any scheduled notifications using their specified transporters.

### ScheduleAndExecute

Schedules the provided notification for all applicable transporters for the provided recipient and attempt to send them straight away.

This method should be used if you want the notifications to ideally be sent at the point of calling it.

## Setup

### AddNotificationService

Service collection extension for adding the notification service and dependencies where you want control how the notifications are stored.

Can be found in **Bottlecap.Net.Notifications** namespace/package.

### AddNotificationServiceWithEF

Service collection extension for adding the notification service and dependencies where EntityFramework is used for storage and retrieval of notifications.

Can be found in **Bottlecap.Net.Notifications.EF** namespace/package.

### Examples

// TODO detailed explaination of setting things up

An example console application for demonstrating the sending of a notification via SendGrid can be found in the [examples section](./src/Examples/Bottlecap.Net.Notifications.ConsoleExample/readme.md).