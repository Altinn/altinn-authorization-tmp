APP=garfield
REGISTRY=acrshared004at21.azurecr.io
TAG=21

all: docker_build docker_push

docker_build:
	docker build \
		-t $(APP) \
		-f Dockerfile \
		../../..

docker_push:
	docker tag $(APP) $(REGISTRY)/$(APP):$(TAG)
	docker push $(REGISTRY)/$(APP):$(TAG)