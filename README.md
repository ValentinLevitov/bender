# Overview
This application - called "Bender" - helps monitor basic tracking and reporting rules for your team in JIRA. For example -

* items assigned to people in a sprint - but still not estimated
* a blocker bug was open a while ago - but still not assigned to a team member for work on it to be started
* a task is "In Progress" for a team member - but with no any updates for 2+ weeks
* and so on...

Bender can also perform simple actions on JIRA issues on his own, for example
* if the issue is assigned by unauthorized person, reassign it (or assign depending on day of week)
* Set automatically DueDate issue field depending on time of a day
* Change issue status depending of linked issue statuses
* and so forth...

Bender is specifically designed to run in containerized environments.

# Quick start
Let's say you want to notify your colleagues about their overdue tasks.
First of all, ensure the following JQL works for you (go to your JIRA and check)

    DueDate < startOfDay() AND Resolution is EMPTY

Of course, you can add a restriction on the JIRA project, issue type, and so on.
If it works and returns non empty list, well, let's move on. We need simple rules file in xml format, place it in some directory

```xml
<!-- /home/user/bender-config/rules.xml file -->
<configuration>
    <jqlRule group="notify-all">
        <jql><![CDATA[ DueDate < startOfDay() AND Resolution is EMPTY ]]></jql>
        <notify 
            mailTo="assignee"
            cc="reporter"
            subject="DueDate of the issue is expired"
            recommendations="Please resolve or reschedule the issue"
            />
    </jqlRule>
</configuration>
```

Adjust your JQL and let's move on.
Next step -- we should point to JIRA instance, email server to use for mailing, let's prepare appsettings.json file

```json
// /home/user/bender-config/appsettings.json file
{
  "Application": {
    "rulesFileName": "/app/rules.xml",
    "supervisors": "enter-your@email.here"
  },

  "Jira": {
    "rootUri": "https://your-jira.server.com",
    "userName": "jira-user-name",
    "password": "jira-user-password",
    "maxResults": 300
  },

  "Smtp": {
    "Host": "smtp.server.com",
    "Port": 587,
    "User": "smtp-user-name",
    "Password": "smtp-user-password",
    "From": "smtp-from-address",
    "EnableSsl": true
  } 
}
```

Now we are ready to run the tool using docker or podman

```bash
$ docker run \
    -v /home/user/bender-config/rules.xml:/app/rules.xml:z \
    -v /home/user/bender-config/appsettings.json:/app/appsettings.json:z \
    -it ghcr.io/valentinlevitov/bender \
    bender notify-all
```

If things goes well, you will see email `DueDate of the issue is expired` with the list of expired issues.
The same email is sent to all assignees of the expired issues (each addressee gets only the issues where she is assignee or reporter).

Good, but it is not very convenient to run tasks manually, so let things happen on their own by a schedule. We should create one more file describing our schedule

```sh
# /home/user/bender-config/crontab file
0 9-20 * * MON-FRI bender notify-all
```
This schedule instructs Bender to start all rules in group="notify-all" every hour from 9:00 to 20:00 by working days, from Monday to Friday. As a scheduler engine Bender uses [supercronic](https://github.com/aptible/supercronic) tool.
Let's start Bender as a daemon with the scheduled job inside
```bash
$ docker run \
    -v /home/user/bender-config/rules.xml:/app/rules.xml:z \
    -v /home/user/bender-config/appsettings.json:/app/appsettings.json:z \
    -v /home/user/bender-config/crontab:/app/crontab:z \
    -it ghcr.io/valentinlevitov/bender \
    supercronic -passthrough-logs /app/crontab
```
From that moment Bender works automatically by schedule until the docker process is stopped.

So far so good. Now let's suppose we want to enhance a bit some workflow process. We guess that issues with type "Support" should be assigned by authorized persons included in team named "Support-Administrators", all other personal should not assign issues. Add new rule in our rules.xml file

```xml
<!-- /home/user/bender-config/rules.xml file -->
<configuration>
    ...
    <jqlRule group="auto-processing">
        <jql><![CDATA[ 
            Type = "Support"
            AND Assignee is Not Empty
            AND (Not Assignee Changed by membersOf("Support-Administrators"))
            AND Resolution is Empty
            ]]>
        </jql>
        <callRest
            verb="PUT"
            urlPattern="{{@jiraRoot}}rest/api/2/issue/{{@issueKey}}">

            <body><![CDATA[
                    {
                        "update": {
                            "assignee": [{"set": {"name": null}}],
                            "comment": [{"add": {"body": "Dropping Assignee. Only members of Support-Administrators team may assign Support issues"}}]
                        }
                    }
                ]]>
            </body>
        </callRest>
     </jqlRule>
</configuration>
```
The rule uses `callRest` action. When called the rule is translated to REST call to the url pointed in `urlPattern` attribute. Supported verbs now are PUT and POST. Placeholder `{{@jiraRoot}}` points to property Jira.rootUri from file appsettings.json. Placeholder `{{@issueKey}}` points to Issue key found for the specified JQL expression.

Add new schedule to the crontab file to start this action automatically lets say every 10 minutes
```sh
# /home/user/bender-config/crontab file
0 9-20 * * MON-FRI bender notify-all
*/10 9-20 * * MON-FRI bender auto-processing
```
Then stop and start docker process again
```bash
$ docker run \
    ...
    supercronic -passthrough-logs /app/crontab
```
# Application Configuration specification
*TODO: /app/appsettings.json and /app/secrets/appsettings.secrets.json files.*

# Rules Configuration specification
*TODO: Supported rule types are: jqlRule (rc), buildRule and structureAmbiguityRule (alpha).*
## Code injection in rule body
*TODO: C# code may be used and placed inside block `<<c#( your-code-here )#>>` in rules.xml file.*

# Logging specification
*TODO: [Serilog](https://github.com/serilog) library is used for the logging, specific configuration should be placed at `Logging` section of the appsettings.json file.*

# Run under Kubernetes, OKD, OpenShift
*TODO: helm chart*