FROM python:3.11-alpine3.18
WORKDIR /app
COPY . /app
RUN pip install -r req.txt
EXPOSE 5001:5000
CMD [ "flask", "run", "--host", "0.0.0.0" ]