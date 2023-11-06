FROM joedwards32/cs2
USER root
RUN set -x \
	# Install, update & upgrade packages
	&& apt-get update \
	&& apt-get install -y --no-install-recommends --no-install-suggests \
		openssh-server \
        && apt-get clean \
        && find /var/lib/apt/lists/ -type f -delete

COPY preentry.sh /preentry.sh
RUN chmod +x /preentry.sh

CMD ["bash", "/preentry.sh"]