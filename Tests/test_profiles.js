/** 
 *	Test for /Exec/Profile/Active endpoint with different params.
 */


import fetch from "node-fetch";

const URL="<YOUR BASE URL>" + "/Exec/Profile/Active";

const APIKEY="<YOUR API KEY>";

async function getActiveProfiles(apiKey, image /* set true to get images*/, complete /*set true to get all the exec info*/)
{
	let urlParam = URL + "?image=" + image + "&complete=" + complete;

	return await (await fetch(urlParam, {
		method: "GET",
		headers: { 
			"Content-Type": "application/json",
			"apikey": apiKey
		}
	})).json();
}


const activeProfiles = await getActiveProfiles(APIKEY, true, true);

console.log(activeProfiles);
