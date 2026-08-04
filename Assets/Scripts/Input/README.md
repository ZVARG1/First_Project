# Input

## Purpose

Responsible for first-launch setup and input configuration.

## Scripts

### InputSaveManager
Loads and saves Input System overrides.

### InputWizardManager
Controls the first-launch wizard.

### InputWizardTransitioner
Transfers control from the wizard to the lobby.

### WizardRebindButton
Handles rebinding for a single input action.

## Flow

Game Launch -> Load Saved Bindings -> Has Wizard Completed? -> Yes / No -> Save Bindings/Input Wizard -> Lobby