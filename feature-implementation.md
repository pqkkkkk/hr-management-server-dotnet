# Overview business logic for feature in module Reward & Activity

## Module Reward

### Create reward program (admin feature)
- Input fields of reward program entity
- List of program policies with input fields of program policy entity
- List of reward item of this reward program with input fields of reward item entity
- Auto creating user wallet for all users in system for this reward program

### View detail reward program (all roles feature)
- Get reward program by id
    - Info of reward program
    - Get list of reward item of this reward program

### Gift point to employee (manager feature)
- List of employee
- Point amount to gift
- Need to select reward program
- Create a `Point Transaction` record
- Notificate employee

### Exchange point to reward item (employee feature)
- Need to select reward program
- List of selected reward item with quantity
- Need to check point balance of user
- Need to check quantity of each selected reward item
- Create a `Point Transaction` record & related `Reward Item Transaction` records


### Fetch and filter point transaction history (employee feature)
- Simply fetch and filter point transaction history

### Fetch and filter point gifted to employee (manager feature)
- Simply fetch and filter point transaction history

### Most important: auto distribute point to employees based on policies of reward programs
- How to define policies of reward programs?
    - Add table `Reward Program Policy`? But how to base on policies to distribute point to employees?
    - Maybe define policy template for all reward programs. Example:
        - Not late policy
        - Attendance policy
        - Overtime policy
        - etc...
    - But how to define policies template applicable to all reward programs?
- How to distribute point to employees? -> Cron job, base on policies of reward programs

### Future features
- Platform for admin to manage reward programs (modify, delete, deactivate, etc...)
- Cron job to deactivate reward programs when end date is reached