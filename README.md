# LangaraCPSC.WebAPI
Web API for langaracpsc-next

## Deployment

### Dependencies
* docker
* docker-compose

### Cloning
```bash
git clone https://github.com/langaracpsc/LangaraCPSC.WebAPI.git
```

### Configuring
```bash
cd LangaraCPSC.WebAPI;

git submodule update --init --remote opendatabaseapi;
git submodule update --init --remote KeyMan;
```

Put the Google service account credentials in `keyfile.json`. The original credentials file can be downloaded from Google cloud console.

Set `CALENDAR_ID` variable in `.env` to the ID of the calendar to fetch from. 

```bash
mkdir data && mkdir data/db;
docker compose -f ./docker-compose.yml up -d; 
```

### Restoring a DB dump
CONTAINER is the id of the postgres container. To get that, run
```
docker container ls
``` 
Look for the container with name `langaracpscwebapi-postgres_image-1`
```bash
cp /path/to/dump data/db; # copy the dump to the volume
docker exec -it -u postgres <CONTAINER> bash; # access the container
```
#### Inside the container
```bash
psql < data/dump;
```

