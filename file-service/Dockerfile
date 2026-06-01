FROM python:3.13-slim

#RUN apt-get update && apt-get install -y \
#    build-essential \
#    libpq-dev \
#    && rm -rf /var/lib/apt/lists/*

ENV PYTHONDONTWRITEBYTECODE=1
ENV PYTHONUNBUFFERED=1

WORKDIR /app

COPY requirements.txt .

RUN pip install --upgrade pip
RUN pip install -r requirements.txt
COPY . .

CMD ["alembic", "upgrade", "head"]

ENV PORT=80
EXPOSE 80
CMD ["sh", "-c", "uvicorn main:app --host 0.0.0.0 --port ${PORT}"]