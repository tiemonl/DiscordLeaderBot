[![CircleCI](https://circleci.com/gh/tiemonl/DiscordLeaderBot.svg?style=svg)](https://circleci.com/gh/tiemonl/DiscordLeaderBot)
# Leaderbot

Leaderbot is a Discord bot created for the purpose of adding achievements to the normal chat experience. Achievements come in the form of roles which give no extra privileges. Achievements are wide ranging from just talking in the server to playing built in games within the bot. As the name suggests, there are leaderboards for a wide range of categories. Being on top of the leaderboard includes a transferable role for the #1 spot.

## Table of contents

TBA

## Server info

[Click to join the server!](https://discord.gg/CV7feUx)

## Commands

 - All commands start with the prefix `-` 
 - `<argument>` = **Required**
 - `(argument)` = *Optional*
 - string arguments with spaces must be enclosed in quotes ""

### Prefixes
- [admin](#admin)
- [bank](#bank)

---

#### admin
***Requires administrator privileges***

| Command | Arguments | Description |
|--|--|--|
| createroles | N/A | This automatically creates the roles from the database into the server. |
| giverole | `<user>` `<roleName>` | Gives specified user the specified role. |
| reorderroles | N/A | This reorders the roles in the server based on difficulty. |
| makerole | `<roleName>` `<roleDescription>` `<roleDifficulty>` | This creates a role in the database. |
| givepoints | `<user>` `<points>` | Gives specified user the specified amount of points |

#### bank
| Command | Arguments | Description |
|--|--|--|
| info | `<bankName>` | Returns information about the bank. |
| takeLoan | `<loanAmount>` `<bankName>` | Requests a loan from the bank. |


## Contributors

- Programmer
	 - @Owl of Moistness#0001 (Discord)
		 - [Github](https://github.com/cesarsld)
	 - @ShadowDragon7015#7015 (Discord)
		 - [Github](https://github.com/ShadowDragon7015)
 - Designer
	 - @Stine#0781 (Discord)
