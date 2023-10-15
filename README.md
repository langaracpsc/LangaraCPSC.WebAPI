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

### Building
```bash
cd LangaraCPSC.WebAPI;

git submodule update --init --remote opendatabaseapi;
git submodule update --init --remote KeyMan;

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

