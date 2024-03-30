## API Endpoints 

* /Exec/ListAll
<pre>
Returns all the Execs in the database. 

Method: GET
Headers: {
    Content-Type: application/json
    apikey: string
}
Permissions: ExecRead
</pre>

* /Exec/Create
<pre>
Creates an Exec and adds it to the database.

Method: POST
Headers: {
    Content-Type: application/json
    apikey: string
} 
Body {
    "studentid": string,
    "firstname": string,
    "email": string,
    "position": int
}

Permissions: ExecRead
</pre>


* /Exec/Update
<pre>
Updates an Exec with the given info. 

Method: POST
Headers: {
    Content-Type: application/json
    apikey: string
} 
Body {
    "studentid": string
    ... 
}
</pre>

* /Exec/End
<pre>
Ends the tenure of the given Exec.

Method: GET
Headers: {
    Content-Type: application/json
    apikey: string
} 
Body {
    "studentid": string
}

Permissions: ExecEnd
</pre>

*  /Exec/Profile/Active?image=false&complete=false
<pre>
Returns the profiles of all the active execs.

Method: GET
Query Params: {
    image: bool,
    complete: bool
}
Headers: {
    Content-Type: application/json
    apikey: string
} 

Permissions: ExecRead
</pre>

*  /Exec/Profile/{id}
<pre>
Returns the profiles of all the active execs.

Method: GET
Route Params: {
    id: int
}
Headers: {
    Content-Type: application/json
    apikey: string
} 

Permissions: ExecRead
</pre>

*  /Exec/Profile/Create
<pre>
Creates an Exec profile with the given info.
Method: GET
Headers: {
    Content-Type: application/json
    apikey: string
} 
Body: {
    "studentid": string,
    "imageid": string,
    "description":  string
}

Permissions: ExecCreate
</pre>

*  /Exec/Profile/Update
<pre>
Updates an Exec profile with the given info.

Method: GET
Headers: {
    Content-Type: application/json
    apikey: string
} 
Body: { 
    "studentid": string,
    "imageid": string,
    "description":  string
}

Permissions: ExecUpdate
</pre>

*  /Exec/Image/{id}
<pre>
Returns the image with the given id.

Method: GET
Route Params: {
    id: int
}
Headers: {
    Content-Type: application/json
    apikey: string
} 

Permissions: ExecRead
</pre>

*  /Exec/Image/Create
<pre>
Creates an image for the given Exec id.

Method: PUT
Headers: {
    Content-Type: application/json
    apikey: string
}
Body: {
    "id": string,
    "buffer": string
}

Permissions: ExecRead
</pre>

* /Event/ListAll
<pre>
Returns all the events in the database. 

Method: GET
Headers: {
    apikey: string
}
Permissions: ExecRead
</pre>

* /Event/{year}/{max}
<pre>
Returns the maximum number of events for a given year.

Method: GET
Route Params: {
    year: int,
    max: int
}
Headers: {
    apikey: string
}
Permissions: ExecRead
</pre>

* /Event/Calendar
<pre>
Returns the calendar invite link for events.

Method: GET
Headers: None
</pre>

* /Event/ICal/{fileName}
<pre>
Returns the iCal file for a given file name.

Method: GET
Route Params: {
    fileName: string
}
Headers: None
</pre> 
