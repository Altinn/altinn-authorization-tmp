#!/usr/bin/env python3
from json import load as load_json
from os import getenv
from argparse import ArgumentParser, Namespace


def main(args: Namespace):
    """
    Reads the given JSON file and appends the key=values to GITHUB_ENV.
    """
    with open(getenv("GITHUB_ENV"), "+a") as f:
        f.writelines(
            map(lambda item: f"{item[0]}={item[1]}\n", read_file(args.file).items())
        )


def read_file(file: str) -> dict:
    if file is None:
        raise ValueError("argument file is not provided")

    with open(file, "+r") as f:
        return load_json(f)


if __name__ == "__main__":
    args = ArgumentParser()
    args.add_argument("-f", "--file", help="file path for metadata.json")
    main(args.parse_args())
